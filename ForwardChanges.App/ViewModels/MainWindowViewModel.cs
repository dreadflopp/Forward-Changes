using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using ForwardChanges.App.Models;
using ForwardChanges.Enums;

namespace ForwardChanges.App.ViewModels;

public sealed class MainWindowViewModel : BindableBase
{
    private StandaloneSettings _settings = new();
    private string _recordSearchText = string.Empty;
    private string _statusText = "Ready";
    private bool _isRunning;
    private string _deepDiveRecordSignaturesText = string.Empty;
    private string _deepDiveFormKeysText = string.Empty;
    private string _deepDivePropertiesText = string.Empty;
    private string _elapsedText = "00:00:00";
    private VirtualMasterRuleViewModel? _selectedCompatibilityRule;

    public MainWindowViewModel()
    {
        RecordTypesView = CollectionViewSource.GetDefaultView(RecordTypes);
        RecordTypesView.Filter = FilterRecordType;
    }

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

    public ICollectionView RecordTypesView { get; }

    public IReadOnlyList<EnumChoice<ProtectionForwardingPolicy>> ProtectionPolicies { get; } =
    [
        new(ProtectionForwardingPolicy.PreferHigherWithAuthorizedDowngrades,
            "Prefer higher, allow explicit downgrades",
            "Prefer Essential over Protected over None, while honoring an intentional downgrade made by a later plugin."),
        new(ProtectionForwardingPolicy.HighestWins,
            "Highest status always wins",
            "Always preserve the highest protection status found anywhere in the override chain."),
        new(ProtectionForwardingPolicy.StandardForwarding,
            "Standard forwarding",
            "Treat the protection flags like ordinary fields and forward the last meaningful change.")
    ];

    public IReadOnlyList<EnumChoice<PerkForwardingPolicy>> PerkPolicies { get; } =
    [
        new(PerkForwardingPolicy.AtomicOnCoupledPropertyChange,
            "Keep coupled PERK data together",
            "When coupled gameplay data changes, take that data from one owning override rather than mixing related fields."),
        new(PerkForwardingPolicy.StandardForwarding,
            "Standard forwarding",
            "Forward registered PERK properties independently.")
    ];

    public IReadOnlyList<EnumChoice<QuestForwardingPolicy>> QuestPolicies { get; } =
    [
        new(QuestForwardingPolicy.AtomicOnStructuralChange,
            "Keep structural QUEST data together",
            "A structural quest change establishes an ownership boundary for the interdependent quest graph."),
        new(QuestForwardingPolicy.StandardForwarding,
            "Standard forwarding",
            "Forward registered QUEST properties independently.")
    ];

    public IReadOnlyList<EnumChoice<StoryManagerForwardingPolicy>> StoryManagerPolicies { get; } =
    [
        new(StoryManagerForwardingPolicy.AtomicOnConfigurationChange,
            "Keep node configuration together",
            "Configuration changes establish ownership of the complete Story Manager node while compatible quest rows may still merge."),
        new(StoryManagerForwardingPolicy.StandardForwarding,
            "Standard forwarding",
            "Forward registered Story Manager properties independently.")
    ];

    public Array LogVerbosityValues => Enum.GetValues<PatcherLogVerbosity>();

    public IReadOnlyList<GameReleaseChoice> GameReleaseChoices => GameReleaseChoice.Supported;

    public string RecordSearchText
    {
        get => _recordSearchText;
        set
        {
            if (SetProperty(ref _recordSearchText, value))
            {
                RecordTypesView.Refresh();
                OnPropertyChanged(nameof(ShownRecordTypeCountText));
            }
        }
    }

    public string SelectedRecordTypeCountText =>
        $"{RecordTypes.Count(x => x.IsEnabled)} of {RecordTypes.Count} enabled";

    public string ShownRecordTypeCountText =>
        string.IsNullOrWhiteSpace(RecordSearchText)
            ? SelectedRecordTypeCountText
            : $"{RecordTypesView.Cast<object>().Count()} shown · {SelectedRecordTypeCountText}";

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
        RecordTypesView.Refresh();
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

    private bool FilterRecordType(object item)
    {
        if (item is not RecordTypeOptionViewModel option || string.IsNullOrWhiteSpace(RecordSearchText))
        {
            return true;
        }

        var search = RecordSearchText.Trim();
        return option.DisplayName.Contains(search, StringComparison.OrdinalIgnoreCase)
            || option.Signature.Contains(search, StringComparison.OrdinalIgnoreCase);
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
        OnPropertyChanged(nameof(ShownRecordTypeCountText));
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
