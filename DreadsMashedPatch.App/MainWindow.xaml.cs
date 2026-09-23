using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using DreadsMashedPatch.App.Models;
using DreadsMashedPatch.App.Services;
using DreadsMashedPatch.App.ViewModels;
using Microsoft.Win32;
using Mutagen.Bethesda.Plugins;

namespace DreadsMashedPatch.App;

public partial class MainWindow : Window
{
    private const int MaxUiLogCharacters = 250_000;

    private readonly MainWindowViewModel _viewModel = new();
    private readonly SettingsStore _settingsStore = new();
    private readonly MasterRuleStore _masterRuleStore = new();
    private readonly PatcherRunner _patcherRunner = new();
    private readonly DispatcherTimer _elapsedTimer;
    private Stopwatch? _runStopwatch;
    private bool _loaded;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
        _elapsedTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(250), DispatcherPriority.Background, OnElapsedTimerTick, Dispatcher);
        _elapsedTimer.Stop();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        try
        {
            var settings = await _settingsStore.LoadAsync();
            _viewModel.Load(settings);

            if (string.IsNullOrWhiteSpace(settings.GameFolderPath)
                || string.IsNullOrWhiteSpace(settings.DataFolderPath)
                || string.IsNullOrWhiteSpace(settings.LoadOrderFilePath))
            {
                DetectPaths(overwriteExisting: false);
            }

            _viewModel.StatusText = "Ready";
        }
        catch (Exception ex)
        {
            _viewModel.Load(new StandaloneSettings());
            MessageBox.Show(
                $"The saved settings could not be loaded. Defaults will be used.\n\n{ex.Message}",
                "Could not load settings",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private async void OnSave(object sender, RoutedEventArgs e)
    {
        if (_viewModel.IsRunning)
        {
            return;
        }

        try
        {
            await SaveSettingsAsync();
            _viewModel.StatusText = "Settings saved";
        }
        catch (Exception ex)
        {
            ShowError("Could not save settings", ex);
        }
    }

    private async void OnRun(object sender, RoutedEventArgs e)
    {
        if (_viewModel.IsRunning)
        {
            return;
        }

        _viewModel.UpdateSettingsFromEditor();
        if (!TryValidateSettings(_viewModel.Settings, out var validationError))
        {
            MessageBox.Show(validationError, "Check patcher settings", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_viewModel.RecordTypes.All(x => !x.IsEnabled)
            && MessageBox.Show(
                "No record types are enabled. The output plugin will contain no forwarded records. Run anyway?",
                "No record types enabled",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await SaveSettingsAsync();
        }
        catch (Exception ex)
        {
            ShowError("The patcher was not started because settings could not be saved", ex);
            return;
        }

        LogSession logSession;
        try
        {
            logSession = LogManager.StartRun(_viewModel.Settings.HistoricalLogsToKeep);
        }
        catch (Exception ex)
        {
            ShowError("The patcher was not started because the portable log could not be created", ex);
            return;
        }

        using (logSession)
        {
            void WriteFullLog(string text) => logSession.Write(text);

            MainTabs.SelectedItem = RunLogTab;
            RunLogTextBox.Clear();
            _viewModel.IsRunning = true;
            _viewModel.StatusText = "Running...";
            _runStopwatch = Stopwatch.StartNew();
            _elapsedTimer.Start();
            UpdateElapsedTime();
            WriteFullLog($"Mashed Patch started at {DateTime.Now:G}{Environment.NewLine}{Environment.NewLine}");
            AppendLog(
                $"Running. Full diagnostic output is being written to:{Environment.NewLine}" +
                $"{logSession.Path}{Environment.NewLine}{Environment.NewLine}");

            try
            {
                var result = await _patcherRunner.RunAsync(_viewModel.Settings, WriteFullLog, AppendLog);
                _runStopwatch.Stop();
                WriteFullLog($"{Environment.NewLine}Completed successfully in {_runStopwatch.Elapsed:g}.{Environment.NewLine}");
                AppendLog(
                    $"{Environment.NewLine}Completed successfully in {_runStopwatch.Elapsed:g}. " +
                    $"Warnings: {result.WarningCount}; errors: {result.ErrorCount}.{Environment.NewLine}");
                _viewModel.StatusText = "Completed";
                MessageBox.Show(
                    "Patch created successfully.",
                    "Mashed Patch completed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _runStopwatch.Stop();
                WriteFullLog($"{Environment.NewLine}FAILED after {_runStopwatch.Elapsed:g}{Environment.NewLine}{ex}{Environment.NewLine}");
                AppendLog(
                    $"{Environment.NewLine}[Error] Patcher failed after {_runStopwatch.Elapsed:g}: {ex.Message}" +
                    $"{Environment.NewLine}See the full diagnostic log for details.{Environment.NewLine}");
                _viewModel.StatusText = "Failed";
                ShowError("The patcher failed. See the Run Log tab and full diagnostic log for details", ex);
            }
            finally
            {
                _elapsedTimer.Stop();
                UpdateElapsedTime();
                _viewModel.IsRunning = false;
            }
        }
    }

    private void OnAutoDetectPaths(object sender, RoutedEventArgs e)
    {
        DetectPaths(overwriteExisting: true);
    }

    private void DetectPaths(bool overwriteExisting)
    {
        var settings = _viewModel.Settings;
        var installations = PathDiscovery.DiscoverInstallations();
        PathDiscovery.Installation? discovery = null;

        if (installations.Count == 1)
        {
            discovery = installations[0];
        }
        else if (installations.Count > 1)
        {
            var picker = new InstallationPickerWindow(installations) { Owner = this };
            if (picker.ShowDialog() == true)
            {
                discovery = picker.SelectedInstallation;
            }
        }

        if (discovery is not null
            && (overwriteExisting || string.IsNullOrWhiteSpace(settings.GameFolderPath)))
        {
            settings.GameRelease = discovery.Release;
            settings.GameFolderPath = discovery.GameFolder;
            settings.DataFolderPath = discovery.DataFolder;
        }

        if (discovery?.LoadOrderFile is not null
            && (overwriteExisting || string.IsNullOrWhiteSpace(settings.LoadOrderFilePath)))
        {
            settings.LoadOrderFilePath = discovery.LoadOrderFile;
        }

        if (discovery is null || discovery.LoadOrderFile is null)
        {
            _viewModel.StatusText = "Some paths could not be detected";
            if (overwriteExisting)
            {
                MessageBox.Show(
                    "One or more Skyrim paths could not be detected. Select the missing paths manually. Mod Organizer 2 users should select the active profile's plugins.txt.",
                    "Automatic detection incomplete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        else
        {
            _viewModel.StatusText = "Paths detected";
        }

    }

    private void OnBrowseGameFolder(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select the Skyrim game folder",
            InitialDirectory = Directory.Exists(_viewModel.Settings.GameFolderPath)
                ? _viewModel.Settings.GameFolderPath
                : null
        };

        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.Settings.GameFolderPath = dialog.FolderName;
            var proposedDataFolder = Path.Combine(dialog.FolderName, "Data");
            if (Directory.Exists(proposedDataFolder))
            {
                _viewModel.Settings.DataFolderPath = proposedDataFolder;
            }

        }
    }

    private void OnBrowseDataFolder(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select the Skyrim Data folder",
            InitialDirectory = Directory.Exists(_viewModel.Settings.DataFolderPath)
                ? _viewModel.Settings.DataFolderPath
                : null
        };

        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.Settings.DataFolderPath = dialog.FolderName;
            var proposedGameFolder = Directory.GetParent(dialog.FolderName)?.FullName;
            if (proposedGameFolder is not null
                && (File.Exists(Path.Combine(proposedGameFolder, "SkyrimSE.exe"))
                    || File.Exists(Path.Combine(proposedGameFolder, "SkyrimVR.exe"))
                    || File.Exists(Path.Combine(proposedGameFolder, "Skyrim.ccc"))))
            {
                _viewModel.Settings.GameFolderPath = proposedGameFolder;
            }

        }
    }

    private void OnBrowseLoadOrder(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select plugins.txt",
            Filter = "Plugin load order (plugins.txt)|plugins.txt|Text files (*.txt)|*.txt|All files (*.*)|*.*",
            CheckFileExists = true,
            FileName = "plugins.txt"
        };

        if (File.Exists(_viewModel.Settings.LoadOrderFilePath))
        {
            dialog.InitialDirectory = Path.GetDirectoryName(_viewModel.Settings.LoadOrderFilePath);
        }

        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.Settings.LoadOrderFilePath = dialog.FileName;
        }
    }

    private void OnEnableAll(object sender, RoutedEventArgs e) => _viewModel.SetAllRecordTypes(true);

    private void OnDisableAll(object sender, RoutedEventArgs e) => _viewModel.SetAllRecordTypes(false);

    private void OnInvert(object sender, RoutedEventArgs e) => _viewModel.InvertRecordTypes();

    private void OnAddCompatibilityRule(object sender, RoutedEventArgs e) =>
        _viewModel.AddCompatibilityRule();

    private void OnRemoveCompatibilityRule(object sender, RoutedEventArgs e) =>
        _viewModel.RemoveSelectedCompatibilityRule();

    private async void OnImportCompatibilityRules(object sender, RoutedEventArgs e)
    {
        if (_viewModel.IsRunning)
        {
            return;
        }

        var dialog = new OpenFileDialog
        {
            Title = "Import master rules",
            Filter = "Master rule files (*.json)|*.json|All files (*.*)|*.*",
            CheckFileExists = true,
            DefaultExt = ".json"
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            var importedRules = await _masterRuleStore.LoadAsync(dialog.FileName);
            if (!TryValidateMasterRules(importedRules, out var validationError))
            {
                MessageBox.Show(
                    validationError,
                    "Invalid master-rule file",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show(
                    $"Replace the current master rules with {importedRules.Count} imported rule(s)?",
                    "Import master rules",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            _viewModel.ReplaceCompatibilityRules(importedRules);
            _viewModel.StatusText = $"Imported {importedRules.Count} master rule(s)";
        }
        catch (Exception ex)
        {
            ShowError("Could not import master rules", ex);
        }
    }

    private async void OnExportCompatibilityRules(object sender, RoutedEventArgs e)
    {
        if (_viewModel.IsRunning)
        {
            return;
        }

        var rules = _viewModel.GetCompatibilityRules();
        if (!TryValidateMasterRules(rules, out var validationError))
        {
            MessageBox.Show(
                validationError,
                "Cannot export master rules",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var dialog = new SaveFileDialog
        {
            Title = "Export master rules",
            Filter = "Master rule files (*.json)|*.json|All files (*.*)|*.*",
            DefaultExt = ".json",
            AddExtension = true,
            OverwritePrompt = true,
            FileName = "MashedPatch.MasterRules.json"
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            await _masterRuleStore.SaveAsync(dialog.FileName, rules);
            _viewModel.StatusText = $"Exported {rules.Count} master rule(s)";
        }
        catch (Exception ex)
        {
            ShowError("Could not export master rules", ex);
        }
    }

    private void OnRestoreDefaultCompatibilityRules(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show(
                "Replace all current master rules with the defaults?",
                "Restore default master rules",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            _viewModel.RestoreDefaultCompatibilityRules();
        }
    }

    private void OnCreateEmptyOutput(object sender, RoutedEventArgs e)
    {
        if (_viewModel.IsRunning)
        {
            return;
        }

        _viewModel.UpdateSettingsFromEditor();
        if (!Directory.Exists(_viewModel.Settings.DataFolderPath))
        {
            MessageBox.Show(
                "Select an existing Skyrim Data folder first.",
                "Check patcher settings",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        if (MessageBox.Show(
                "This will delete the current patch output, then create one empty patch plugin. Continue?",
                "Create empty patch output",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            var result = _patcherRunner.CreateEmptyOutput(_viewModel.Settings);
            _viewModel.StatusText = "Empty patch created";
            MessageBox.Show(
                $"Created an empty patch at:{Environment.NewLine}{result.OutputPath}{Environment.NewLine}{Environment.NewLine}" +
                $"Removed previous output files: {result.RemovedOutputCount}",
                "Empty patch created",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            ShowError("Could not create the empty patch output", ex);
        }
    }

    private void OnRecordTypeListPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Space || RecordTypeList.SelectedItem is not RecordTypeOptionViewModel focusedOption)
        {
            return;
        }

        var enable = !focusedOption.IsEnabled;
        foreach (var option in RecordTypeList.SelectedItems.Cast<RecordTypeOptionViewModel>())
        {
            option.IsEnabled = enable;
        }

        e.Handled = true;
    }

    private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
        {
            OnSave(sender, e);
            e.Handled = true;
        }
        else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.R)
        {
            OnRun(sender, e);
            e.Handled = true;
        }
    }

    private void OnCopyLog(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(RunLogTextBox.Text))
        {
            Clipboard.SetText(RunLogTextBox.Text);
            _viewModel.StatusText = "Visible messages copied";
        }
    }

    private void OnOpenLogsFolder(object sender, RoutedEventArgs e)
    {
        Directory.CreateDirectory(LogManager.LogsDirectory);
        Process.Start(new ProcessStartInfo
        {
            FileName = LogManager.LogsDirectory,
            UseShellExecute = true
        });
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (!_viewModel.IsRunning)
        {
            return;
        }

        MessageBox.Show(
            "The patcher is still running. Wait for it to finish before closing Mashed Patch.",
            "Patcher is running",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
        e.Cancel = true;
    }

    private async Task SaveSettingsAsync()
    {
        _viewModel.UpdateSettingsFromEditor();
        await _settingsStore.SaveAsync(_viewModel.Settings);
    }

    private void AppendLog(string text)
    {
        Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() =>
            {
                RunLogTextBox.AppendText(text);
                TrimUiLog();
                RunLogTextBox.ScrollToEnd();
            }));
    }

    private void TrimUiLog()
    {
        var excessCharacters = RunLogTextBox.Text.Length - MaxUiLogCharacters;
        if (excessCharacters <= 0)
        {
            return;
        }

        var firstCompleteLine = RunLogTextBox.Text.IndexOf('\n', excessCharacters);
        var charactersToRemove = firstCompleteLine >= 0
            ? firstCompleteLine + 1
            : excessCharacters;
        RunLogTextBox.Select(0, charactersToRemove);
        RunLogTextBox.SelectedText = string.Empty;
    }

    private void OnElapsedTimerTick(object? sender, EventArgs e) => UpdateElapsedTime();

    private void UpdateElapsedTime()
    {
        var elapsed = _runStopwatch?.Elapsed ?? TimeSpan.Zero;
        _viewModel.ElapsedText = $"{(int)elapsed.TotalHours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}";
    }

    private static bool TryValidateSettings(StandaloneSettings settings, out string error)
    {
        if (!Directory.Exists(settings.GameFolderPath))
        {
            error = "Select an existing Skyrim game folder.";
            return false;
        }

        if (!File.Exists(Path.Combine(settings.GameFolderPath, "SkyrimSE.exe"))
            && !File.Exists(Path.Combine(settings.GameFolderPath, "SkyrimVR.exe")))
        {
            error = "The selected game folder does not contain SkyrimSE.exe or SkyrimVR.exe.";
            return false;
        }

        if (!Directory.Exists(settings.DataFolderPath))
        {
            error = "Select an existing Skyrim Data folder.";
            return false;
        }

        if (!File.Exists(Path.Combine(settings.DataFolderPath, "Skyrim.esm")))
        {
            error = "The selected Data folder does not contain Skyrim.esm.";
            return false;
        }

        if (!File.Exists(settings.LoadOrderFilePath))
        {
            error = "Select an existing plugins.txt load-order file.";
            return false;
        }

        var invalidIgnoredMod = settings.Patcher.IgnoredMods
            .FirstOrDefault(name => !ModKey.TryFromFileName(name, out _));
        if (invalidIgnoredMod is not null)
        {
            error = $"The ignored-plugins list contains an invalid plugin filename: {invalidIgnoredMod}";
            return false;
        }

        var invalidAlwaysWinningMod = settings.Patcher.AlwaysWinningMods
            .FirstOrDefault(name => !ModKey.TryFromFileName(name, out _));
        if (invalidAlwaysWinningMod is not null)
        {
            error = $"The always-win list contains an invalid plugin filename: {invalidAlwaysWinningMod}";
            return false;
        }

        var contradictoryMod = settings.Patcher.AlwaysWinningMods
            .FirstOrDefault(settings.Patcher.IgnoredMods.Contains);
        if (contradictoryMod is not null)
        {
            error = $"A plugin cannot be both ignored and configured to always win: {contradictoryMod}";
            return false;
        }

        var invalidWeaponTypeKeyword = settings.Patcher.Forwarding.VanillaWeaponTypeKeywords
            .FirstOrDefault(value => !FormKey.TryFactory(value.AsSpan(), out _));
        if (invalidWeaponTypeKeyword is not null)
        {
            error = $"The vanilla weapon type keyword list contains an invalid FormKey: {invalidWeaponTypeKeyword}";
            return false;
        }

        if (!TryValidateMasterRules(settings.Patcher.CompatibilityRules, out error))
        {
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static bool TryValidateMasterRules(
        IReadOnlyList<VirtualMasterRule> rules,
        out string error)
    {
        for (var index = 0; index < rules.Count; index++)
        {
            var rule = rules[index];
            if (!ModKey.TryFromFileName(rule.InjectedMaster, out _))
            {
                error = $"Master rule {index + 1} has an invalid filename for the plugin to treat as a master.";
                return false;
            }

            var invalidTarget = rule.TargetMods.FirstOrDefault(target => !ModKey.TryFromFileName(target, out _));
            if (invalidTarget is not null)
            {
                error = $"Master rule {index + 1} has an invalid target plugin filename: {invalidTarget}";
                return false;
            }
        }

        error = string.Empty;
        return true;
    }

    private static void ShowError(string heading, Exception exception)
    {
        MessageBox.Show(
            $"{heading}.\n\n{exception.Message}",
            "Mashed Patch",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}
