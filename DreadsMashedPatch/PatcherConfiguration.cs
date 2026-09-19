using DreadsMashedPatch.Enums;

namespace DreadsMashedPatch;

public sealed class PatcherConfiguration
{
    /// <summary>
    /// Full interface names for record families that should not be processed.
    /// An empty set means every supported record family. Storing exclusions keeps
    /// newly added record families enabled by default after an application update.
    /// </summary>
    public HashSet<string> DisabledRecordTypes { get; set; } = new(StringComparer.Ordinal);

    public ForwardingSettings Forwarding { get; set; } = new();

    public DiagnosticsSettings Diagnostics { get; set; } = new();

    public List<VirtualMasterRule> CompatibilityRules { get; set; } = [];

    public void Normalize()
    {
        Forwarding ??= new ForwardingSettings();
        Diagnostics ??= new DiagnosticsSettings();
        Diagnostics.Normalize();
        CompatibilityRules = (CompatibilityRules ?? [])
            .Where(rule => rule is not null)
            .Select(rule => rule.Normalize())
            .ToList();

        DisabledRecordTypes = new HashSet<string>(
            (DisabledRecordTypes ?? []).Where(x => !string.IsNullOrWhiteSpace(x)),
            StringComparer.Ordinal);
    }

    public PatcherConfiguration Copy()
    {
        Normalize();
        return new PatcherConfiguration
        {
            DisabledRecordTypes = new HashSet<string>(DisabledRecordTypes, StringComparer.Ordinal),
            Forwarding = new ForwardingSettings
            {
                TreatCreationClubAsVanilla = Forwarding.TreatCreationClubAsVanilla,
                ProtectionPolicy = Forwarding.ProtectionPolicy,
                PerkPolicy = Forwarding.PerkPolicy,
                QuestPolicy = Forwarding.QuestPolicy,
                StoryManagerPolicy = Forwarding.StoryManagerPolicy
            },
            Diagnostics = Diagnostics.Copy(),
            CompatibilityRules = CompatibilityRules.Select(rule => rule.Copy()).ToList()
        };
    }
}

public sealed class VirtualMasterRule
{
    public string InjectedMaster { get; set; } = string.Empty;

    public HashSet<string> TargetMods { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public VirtualMasterRule Normalize()
    {
        InjectedMaster = InjectedMaster?.Trim() ?? string.Empty;
        TargetMods = new HashSet<string>(
            (TargetMods ?? []).Select(name => name.Trim()).Where(name => name.Length > 0),
            StringComparer.OrdinalIgnoreCase);
        return this;
    }

    public VirtualMasterRule Copy() => new()
    {
        InjectedMaster = InjectedMaster,
        TargetMods = new HashSet<string>(TargetMods, StringComparer.OrdinalIgnoreCase)
    };
}

public sealed class ForwardingSettings
{
    public bool TreatCreationClubAsVanilla { get; set; } = true;

    public ProtectionForwardingPolicy ProtectionPolicy { get; set; } =
        ProtectionForwardingPolicy.PreferHigherWithAuthorizedDowngrades;

    public PerkForwardingPolicy PerkPolicy { get; set; } =
        PerkForwardingPolicy.AtomicOnCoupledPropertyChange;

    public QuestForwardingPolicy QuestPolicy { get; set; } =
        QuestForwardingPolicy.AtomicOnStructuralChange;

    public StoryManagerForwardingPolicy StoryManagerPolicy { get; set; } =
        StoryManagerForwardingPolicy.AtomicOnConfigurationChange;
}

public sealed class DiagnosticsSettings
{
    public bool DebugMode { get; set; }

    public PatcherLogVerbosity Verbosity { get; set; } = PatcherLogVerbosity.ContextChanges;

    public bool EnableStartupDiagnostics { get; set; }

    public bool IncludeNoChangeDecisionsInDetailed { get; set; } = true;

    public int MaxValuePreviewLength { get; set; } = 240;

    public HashSet<string> DeepDiveRecordSignatures { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public HashSet<string> DeepDiveFormKeys { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public HashSet<string> DeepDiveProperties { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public void Normalize()
    {
        MaxValuePreviewLength = Math.Clamp(MaxValuePreviewLength, 40, 10_000);
        DeepDiveRecordSignatures = NormalizeSet(DeepDiveRecordSignatures);
        DeepDiveFormKeys = NormalizeSet(DeepDiveFormKeys);
        DeepDiveProperties = NormalizeSet(DeepDiveProperties);
    }

    public DiagnosticsSettings Copy()
    {
        Normalize();
        return new DiagnosticsSettings
        {
            DebugMode = DebugMode,
            Verbosity = Verbosity,
            EnableStartupDiagnostics = EnableStartupDiagnostics,
            IncludeNoChangeDecisionsInDetailed = IncludeNoChangeDecisionsInDetailed,
            MaxValuePreviewLength = MaxValuePreviewLength,
            DeepDiveRecordSignatures = new HashSet<string>(DeepDiveRecordSignatures, StringComparer.OrdinalIgnoreCase),
            DeepDiveFormKeys = new HashSet<string>(DeepDiveFormKeys, StringComparer.OrdinalIgnoreCase),
            DeepDiveProperties = new HashSet<string>(DeepDiveProperties, StringComparer.OrdinalIgnoreCase)
        };
    }

    private static HashSet<string> NormalizeSet(IEnumerable<string>? values)
    {
        return new HashSet<string>(
            (values ?? []).Select(x => x.Trim()).Where(x => x.Length > 0),
            StringComparer.OrdinalIgnoreCase);
    }
}
