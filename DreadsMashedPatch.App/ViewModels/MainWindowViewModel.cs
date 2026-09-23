using System.Collections.ObjectModel;
using System.ComponentModel;
using DreadsMashedPatch.App.Models;
using DreadsMashedPatch.Enums;

namespace DreadsMashedPatch.App.ViewModels;

public sealed class MainWindowViewModel : BindableBase
{
    private StandaloneSettings _settings = new();
    private string _statusText = "Ready";
    private bool _isRunning;
    private string _deepDiveRecordSignaturesText = string.Empty;
    private string _deepDiveFormKeysText = string.Empty;
    private string _deepDivePropertiesText = string.Empty;
    private string _ignoredModsText = string.Empty;
    private string _alwaysWinningModsText = string.Empty;
    private string _vanillaWeaponTypeKeywordsText = string.Empty;
    private string _elapsedText = "00:00:00";
    private VirtualMasterRuleViewModel? _selectedCompatibilityRule;

    public StandaloneSettings Settings
    {
        get => _settings;
        private set => SetProperty(ref _settings, value);
    }

    public ObservableCollection<RecordTypeOptionViewModel> RecordTypes { get; } = [];

    public ObservableCollection<VirtualMasterRuleViewModel> CompatibilityRules { get; } = [];

    public IReadOnlyList<EnumChoice<EditorIdForwardingPolicy>> EditorIdPolicies { get; } =
    [
        new(EditorIdForwardingPolicy.ForwardOnlyWithOtherChanges,
            "Forward only on an existing patch record (Recommended)"),
        new(EditorIdForwardingPolicy.PreserveBaseline,
            "Preserve the official Editor ID"),
        new(EditorIdForwardingPolicy.StandardForwarding,
            "Forward Editor ID changes")
    ];

    public VirtualMasterRuleViewModel? SelectedCompatibilityRule
    {
        get => _selectedCompatibilityRule;
        set => SetProperty(ref _selectedCompatibilityRule, value);
    }

    public IReadOnlyList<EnumChoice<ProtectionForwardingPolicy>> ProtectionPolicies { get; } =
    [
        new(ProtectionForwardingPolicy.PreferHigherWithAuthorizedDowngrades,
            "Protect NPCs unless deliberately changed (Recommended)"),
        new(ProtectionForwardingPolicy.HighestWins,
            "Always keep the strongest protection"),
        new(ProtectionForwardingPolicy.StandardForwarding,
            "Forward each status change normally")
    ];

    public IReadOnlyList<EnumChoice<PatcherLogVerbosity>> LogVerbosityChoices { get; } =
    [
        new(PatcherLogVerbosity.Summary, "Summary only"),
        new(PatcherLogVerbosity.ContextChanges, "Changed records")
    ];

    public IReadOnlyList<GameReleaseChoice> GameReleaseChoices => GameReleaseChoice.Supported;

    public string SelectedRecordTypeCountText =>
        $"{RecordTypes.Count(x => x.IsEnabled)} of {RecordTypes.Count} enabled";

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public string ElapsedText
    {
        get => _elapsedText;
        set => SetProperty(ref _elapsedText, value);
    }

    public bool IsRunning
    {
        get => _isRunning;
        set => SetProperty(ref _isRunning, value);
    }

    public string DeepDiveRecordSignaturesText
    {
        get => _deepDiveRecordSignaturesText;
        set => SetProperty(ref _deepDiveRecordSignaturesText, value);
    }

    public string DeepDiveFormKeysText
    {
        get => _deepDiveFormKeysText;
        set => SetProperty(ref _deepDiveFormKeysText, value);
    }

    public string DeepDivePropertiesText
    {
        get => _deepDivePropertiesText;
        set => SetProperty(ref _deepDivePropertiesText, value);
    }

    public string IgnoredModsText
    {
        get => _ignoredModsText;
        set => SetProperty(ref _ignoredModsText, value);
    }

    public string AlwaysWinningModsText
    {
        get => _alwaysWinningModsText;
        set => SetProperty(ref _alwaysWinningModsText, value);
    }

    public string VanillaWeaponTypeKeywordsText
    {
        get => _vanillaWeaponTypeKeywordsText;
        set => SetProperty(ref _vanillaWeaponTypeKeywordsText, value);
    }

    public void Load(StandaloneSettings settings)
    {
        settings.Normalize();
        if (settings.Patcher.Diagnostics.Verbosity == PatcherLogVerbosity.Detailed)
        {
            settings.Patcher.Diagnostics.Verbosity = PatcherLogVerbosity.ContextChanges;
        }

        Settings = settings;
        RecordTypes.Clear();
        CompatibilityRules.Clear();

        var groupedTypes = Program.SupportedRecordTypes
            .GroupBy(RecordTypeCatalog.GetSignature)
            .OrderBy(group => RecordTypeCatalog.GetGroupedDisplayName(group.Key, group));

        foreach (var group in groupedTypes)
        {
            var recordTypes = group.ToArray();
            var enabled = recordTypes.All(recordType =>
                !settings.Patcher.DisabledRecordTypes.Contains(recordType.FullName ?? recordType.Name));
            var option = new RecordTypeOptionViewModel(
                group.Key,
                RecordTypeCatalog.GetGroupedDisplayName(group.Key, recordTypes),
                recordTypes,
                enabled);
            option.PropertyChanged += OnRecordTypePropertyChanged;
            RecordTypes.Add(option);
        }

        DeepDiveRecordSignaturesText = JoinLines(settings.Patcher.Diagnostics.DeepDiveRecordSignatures);
        DeepDiveFormKeysText = JoinLines(settings.Patcher.Diagnostics.DeepDiveFormKeys);
        DeepDivePropertiesText = JoinLines(settings.Patcher.Diagnostics.DeepDiveProperties);
        IgnoredModsText = JoinLines(settings.Patcher.IgnoredMods);
        AlwaysWinningModsText = JoinLinesInOrder(settings.Patcher.AlwaysWinningMods);
        VanillaWeaponTypeKeywordsText = JoinLines(
            settings.Patcher.Forwarding.VanillaWeaponTypeKeywords);
        foreach (var rule in settings.Patcher.CompatibilityRules)
        {
            CompatibilityRules.Add(new VirtualMasterRuleViewModel(rule));
        }
        SelectedCompatibilityRule = CompatibilityRules.FirstOrDefault();
        NotifyRecordCounts();
    }

    public void UpdateSettingsFromEditor()
    {
        Settings.Patcher.DisabledRecordTypes = RecordTypes
            .Where(x => !x.IsEnabled)
            .SelectMany(x => x.Ids)
            .ToHashSet(StringComparer.Ordinal);
        Settings.Patcher.Diagnostics.DeepDiveRecordSignatures = ParseEntries(DeepDiveRecordSignaturesText);
        Settings.Patcher.Diagnostics.DeepDiveFormKeys = ParseEntries(DeepDiveFormKeysText);
        Settings.Patcher.Diagnostics.DeepDiveProperties = ParseEntries(DeepDivePropertiesText);
        Settings.Patcher.IgnoredMods = ParseEntries(IgnoredModsText);
        Settings.Patcher.AlwaysWinningMods = ParseOrderedEntries(AlwaysWinningModsText);
        Settings.Patcher.Forwarding.VanillaWeaponTypeKeywords =
            ParseEntries(VanillaWeaponTypeKeywordsText);
        Settings.Patcher.CompatibilityRules = GetCompatibilityRules();
        Settings.Normalize();
    }

    public List<VirtualMasterRule> GetCompatibilityRules() =>
        CompatibilityRules.Select(rule => rule.ToModel().Normalize()).ToList();

    public void ReplaceCompatibilityRules(IEnumerable<VirtualMasterRule> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        CompatibilityRules.Clear();
        foreach (var rule in rules)
        {
            CompatibilityRules.Add(new VirtualMasterRuleViewModel(rule.Copy().Normalize()));
        }

        SelectedCompatibilityRule = CompatibilityRules.FirstOrDefault();
    }

    public void AddCompatibilityRule()
    {
        var rule = new VirtualMasterRuleViewModel(new VirtualMasterRule());
        CompatibilityRules.Add(rule);
        SelectedCompatibilityRule = rule;
    }

    public void RestoreDefaultCompatibilityRules()
    {
        ReplaceCompatibilityRules(PatcherConfiguration.CreateDefaultCompatibilityRules());
    }

    public void RemoveSelectedCompatibilityRule()
    {
        if (SelectedCompatibilityRule is null)
        {
            return;
        }

        var index = CompatibilityRules.IndexOf(SelectedCompatibilityRule);
        CompatibilityRules.Remove(SelectedCompatibilityRule);
        SelectedCompatibilityRule = CompatibilityRules.Count == 0
            ? null
            : CompatibilityRules[Math.Min(index, CompatibilityRules.Count - 1)];
    }

    public void SetAllRecordTypes(bool enabled)
    {
        foreach (var option in RecordTypes)
        {
            option.IsEnabled = enabled;
        }
    }

    public void InvertRecordTypes()
    {
        foreach (var option in RecordTypes)
        {
            option.IsEnabled = !option.IsEnabled;
        }
    }

    private void OnRecordTypePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RecordTypeOptionViewModel.IsEnabled))
        {
            NotifyRecordCounts();
        }
    }

    private void NotifyRecordCounts()
    {
        OnPropertyChanged(nameof(SelectedRecordTypeCountText));
    }

    private static HashSet<string> ParseEntries(string text)
    {
        return text.Split(['\r', '\n', ',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => x.Length > 0)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static List<string> ParseOrderedEntries(string text) =>
        text.Split(['\r', '\n', ',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => x.Length > 0)
            .Reverse()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Reverse()
            .ToList();

    private static string JoinLines(IEnumerable<string> values) =>
        string.Join(Environment.NewLine, values.Order(StringComparer.OrdinalIgnoreCase));

    private static string JoinLinesInOrder(IEnumerable<string> values) =>
        string.Join(Environment.NewLine, values);

}
