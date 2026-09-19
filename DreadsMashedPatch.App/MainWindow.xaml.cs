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
    private readonly MainWindowViewModel _viewModel = new();
    private readonly SettingsStore _settingsStore = new();
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
            void WriteRunLog(string text)
            {
                logSession.Write(text);
                AppendLog(text);
            }

            MainTabs.SelectedItem = RunLogTab;
            RunLogTextBox.Clear();
            _viewModel.IsRunning = true;
            _viewModel.StatusText = "Running...";
            _runStopwatch = Stopwatch.StartNew();
            _elapsedTimer.Start();
            UpdateElapsedTime();
            WriteRunLog($"Dread's Mashed Patch started at {DateTime.Now:G}{Environment.NewLine}{Environment.NewLine}");

            try
            {
                await _patcherRunner.RunAsync(_viewModel.Settings, WriteRunLog);
                _runStopwatch.Stop();
                WriteRunLog($"{Environment.NewLine}Completed successfully in {_runStopwatch.Elapsed:g}.{Environment.NewLine}");
                _viewModel.StatusText = "Completed";
                MessageBox.Show(
                    "Patch created successfully.",
                    "Dread's Mashed Patch completed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _runStopwatch.Stop();
                WriteRunLog($"{Environment.NewLine}FAILED after {_runStopwatch.Elapsed:g}{Environment.NewLine}{ex}{Environment.NewLine}");
                _viewModel.StatusText = "Failed";
                ShowError("The patcher failed. See the Run Log tab for details", ex);
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

    private void OnAddSimonRimExampleRule(object sender, RoutedEventArgs e) =>
        _viewModel.AddSimonRimExampleRule();

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
            _viewModel.StatusText = "Log copied";
        }
    }

    private void OnClearLog(object sender, RoutedEventArgs e) => RunLogTextBox.Clear();

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
            "The patcher is still running. Wait for it to finish before closing Dread's Mashed Patch.",
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
        Dispatcher.BeginInvoke(() =>
        {
            RunLogTextBox.AppendText(text);
            RunLogTextBox.ScrollToEnd();
        });
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

        for (var index = 0; index < settings.Patcher.CompatibilityRules.Count; index++)
        {
            var rule = settings.Patcher.CompatibilityRules[index];
            if (!ModKey.TryFromFileName(rule.InjectedMaster, out _))
            {
                error = $"Master rule {index + 1} has an invalid filename for the plugin to treat as a master.";
                return false;
            }

            if (rule.TargetMods.Count == 0)
            {
                error = $"Master rule {index + 1} must contain at least one target mod.";
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
            "Dread's Mashed Patch",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}
