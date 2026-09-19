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
    private string _elapsedText = "00:00:00";
    private VirtualMasterRuleViewModel? _selectedCompatibilityRule;

    public StandaloneSettings Settings
    {
        get => _settings;
        private set => SetProperty(ref _settings, value);
    }

    public ObservableCollection<RecordTypeOptionViewModel> RecordTypes { get; } = [];

    public ObservableCollection<VirtualMasterRuleViewModel> CompatibilityRules { get; } = [];

    public VirtualMasterRuleViewModel? SelectedCompatibilityRule
    {
        get => _selectedCompatibilityRule;
        set => SetProperty(ref _selectedCompatibilityRule, value);
    }

    public IReadOnlyList<EnumChoice<ProtectionForwardingPolicy>> ProtectionPolicies { get; } =
    [
        new(ProtectionForwardingPolicy.PreferHigherWithAuthorizedDowngrades,
            "Protect NPCs unless deliberately changed (Recommended)",
            "Prefer Essential over Protected over None, while honoring an intentional downgrade made by a later plugin."),
        new(ProtectionForwardingPolicy.HighestWins,
            "Always keep the strongest protection",
            "Always preserve the highest protection status found anywhere in the override chain."),
        new(ProtectionForwardingPolicy.StandardForwarding,
            "Forward each status change normally",
            "Treat the protection flags like ordinary fields and forward the last meaningful change.")
    ];

    public IReadOnlyList<EnumChoice<PerkForwardingPolicy>> PerkPolicies { get; } =
    [
        new(PerkForwardingPolicy.AtomicOnCoupledPropertyChange,
            "Keep related perk data together (Recommended)",
            "When coupled gameplay data changes, take that data from one owning override rather than mixing related fields."),
        new(PerkForwardingPolicy.StandardForwarding,
            "Forward perk fields separately",
            "Forward registered PERK properties independently.")
    ];

    public IReadOnlyList<EnumChoice<QuestForwardingPolicy>> QuestPolicies { get; } =
    [
        new(QuestForwardingPolicy.AtomicOnStructuralChange,
            "Keep related quest data together (Recommended)",
            "A structural quest change establishes an ownership boundary for the interdependent quest graph."),
        new(QuestForwardingPolicy.StandardForwarding,
            "Forward quest fields separately",
            "Forward registered QUEST properties independently.")
    ];

    public IReadOnlyList<EnumChoice<StoryManagerForwardingPolicy>> StoryManagerPolicies { get; } =
    [
        new(StoryManagerForwardingPolicy.AtomicOnConfigurationChange,
            "Keep each node's setup together (Recommended)",
            "Configuration changes establish ownership of the complete Story Manager node while compatible quest rows may still merge."),
        new(StoryManagerForwardingPolicy.StandardForwarding,
            "Forward node fields separately",
            "Forward registered Story Manager properties independently.")
    ];

    public IReadOnlyList<EnumChoice<PatcherLogVerbosity>> LogVerbosityChoices { get; } =
    [
        new(PatcherLogVerbosity.Summary, "Summary only", "Show progress and warnings."),
        new(PatcherLogVerbosity.ContextChanges, "Changed records", "Also show decisions for records whose source changes.")
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
        Settings.Patcher.CompatibilityRules = CompatibilityRules.Select(rule => rule.ToModel()).ToList();
        Settings.Normalize();
    }

    public void AddCompatibilityRule()
    {
        var rule = new VirtualMasterRuleViewModel(new VirtualMasterRule());
        CompatibilityRules.Add(rule);
        SelectedCompatibilityRule = rule;
    }

    public void AddSimonRimExampleRule()
    {
        const string unofficialPatch = "Unofficial Skyrim Special Edition Patch.esp";
        string[] simonRimPlugins =
        [
            "Adamant.esp",
            "MysticismMagic.esp",
            "Aetherius.esp",
            "Mundus.esp",
            "BladeAndBlunt.esp",
            "Apothecary.esp"
        ];

        var rule = CompatibilityRules.FirstOrDefault(candidate =>
            string.Equals(candidate.InjectedMaster.Trim(), unofficialPatch, StringComparison.OrdinalIgnoreCase));
        if (rule is null)
        {
            rule = new VirtualMasterRuleViewModel(new VirtualMasterRule
            {
                InjectedMaster = unofficialPatch
            });
            CompatibilityRules.Add(rule);
        }

        var targetMods = rule.ToModel().TargetMods;
        targetMods.UnionWith(simonRimPlugins);
        rule.TargetModsText = string.Join(Environment.NewLine, targetMods.Order(StringComparer.OrdinalIgnoreCase));
        SelectedCompatibilityRule = rule;
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

    private static string JoinLines(IEnumerable<string> values) =>
        string.Join(Environment.NewLine, values.Order(StringComparer.OrdinalIgnoreCase));

}
