using DreadsMashedPatch.Enums;
using Mutagen.Bethesda.Skyrim;

namespace DreadsMashedPatch;

public sealed class PatcherConfiguration
{
    /// <summary>
    /// Full interface names for record families that should not be processed.
    /// Structurally coupled dialogue, navigation, and package record families default
    /// to disabled because independently forwarded fields can create combinations that
    /// no source plugin authored or, in the case of packages, cannot currently be
    /// written without reordering indexed data.
    /// Storing exclusions keeps newly added record families enabled by default after
    /// an application update.
    /// </summary>
    public HashSet<string> DisabledRecordTypes { get; set; } = new(StringComparer.Ordinal)
    {
        typeof(IDialogTopicGetter).FullName!,
        typeof(IDialogBranchGetter).FullName!,
        typeof(IDialogResponsesGetter).FullName!,
        typeof(IDialogViewGetter).FullName!,
        typeof(INavigationMeshGetter).FullName!,
        typeof(IPackageGetter).FullName!
    };

    public ForwardingSettings Forwarding { get; set; } = new();

    public DiagnosticsSettings Diagnostics { get; set; } = new();

    /// <summary>
    /// Plugins whose overrides are omitted while conflict chains are evaluated.
    /// </summary>
    public HashSet<string> IgnoredMods { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "True Light.esp",
        "True Light - USSEP Patch.esp"
    };

    /// <summary>
    /// Plugins whose complete record snapshots take precedence whenever another
    /// plugin overwrites them. If several configured plugins contain the same
    /// record, the plugin occurring last in this list takes precedence.
    /// </summary>
    public List<string> AlwaysWinningMods { get; set; } = [];

    public List<VirtualMasterRule> CompatibilityRules { get; set; } = CreateDefaultCompatibilityRules();

    public static List<VirtualMasterRule> CreateDefaultCompatibilityRules() =>
    [
        new VirtualMasterRule
        {
            InjectedMaster = "Unofficial Skyrim Special Edition Patch.esp",
            TargetMods =
            [
                "imp_helm_legend.esp",
                "Navigator-NavFixes.esl",
                "SurvivalModeImproved.esp",
                "King-Priest.esp",
                "Window Shadows Ultimate.esp"
            ]
        },
        new VirtualMasterRule
        {
            InjectedMaster = "Unofficial Skyrim Creation Club Content Patch.esl",
            TargetMods =
            [
                "Creation Club Rebalancing.esp",
                "Masterwork - Bittercup.esp",
                "Masterwork - Chrysamere.esp",
                "Masterwork - Civil War Champions.esp",
                "Masterwork - Dawnfang.esp",
                "Masterwork - Dead Man's Dread.esp",
                "Masterwork - Fishing.esp",
                "Masterwork - Forgotten Seasons.esp",
                "Masterwork - Gallows Hall - Tweaks and Enhancements.esp",
                "Masterwork - Gallows Hall.esp",
                "Masterwork - Ghosts of the Tribunal - Reduced Cut.esp",
                "Masterwork - Ghosts of the Tribunal.esp",
                "Masterwork - Goldbrand.esp",
                "Masterwork - Ruin's Edge.esp",
                "Masterwork - Saints and Seducers.esp",
                "Masterwork - Shadowrend.esp",
                "Masterwork - Spell Knight Armor.esp",
                "Masterwork - Stendarr's Hammer.esp",
                "Masterwork - Sunder and Wraithguard.esp",
                "Masterwork - The Arms of Chaos.esp",
                "Masterwork - The Boots of Blinding Speed.esp",
                "Masterwork - The Bow of Shadows.esp",
                "Masterwork - The Cause.esp",
                "Masterwork - The Contest.esp",
                "Masterwork - The Crusader's Relics - Knight of the North.esp",
                "Masterwork - The Crusader's Relics.esp",
                "Masterwork - The Dragonbone Mail.esp",
                "Masterwork - The Gray Cowl.esp",
                "Masterwork - The Headman's Cleaver.esp",
                "Masterwork - The Lord's Mail.esp",
                "Masterwork - The Staff of Hasedoki.esp",
                "Masterwork - The Staff of Sheogorath - ECSS.esp",
                "Masterwork - The Staff of Sheogorath.esp",
                "Masterwork - Umbra.esp",
                "Starfrost.esp"
            ]
        },
         new VirtualMasterRule
        {
            InjectedMaster = "Apothecary.esp",
            TargetMods =
            [
                "StarfrostInjuries.esp"
            ]
        },
        new VirtualMasterRule
        {
            InjectedMaster = "BSHeartland - Unofficial Fixes.esp",
            TargetMods =
            [
                "BS Bruma - CC Curios Patch.esp"
            ]
        }
    ];

    public void Normalize()
    {
        Forwarding ??= new ForwardingSettings();
        Forwarding.Normalize();
        Diagnostics ??= new DiagnosticsSettings();
        Diagnostics.Normalize();
        CompatibilityRules = (CompatibilityRules ?? [])
            .Where(rule => rule is not null)
            .Select(rule => rule.Normalize())
            .ToList();

        DisabledRecordTypes = new HashSet<string>(
            (DisabledRecordTypes ?? []).Where(x => !string.IsNullOrWhiteSpace(x)),
            StringComparer.Ordinal);
        IgnoredMods = new HashSet<string>(
            (IgnoredMods ?? []).Select(name => name.Trim()).Where(name => name.Length > 0),
            StringComparer.OrdinalIgnoreCase);
        AlwaysWinningMods = (AlwaysWinningMods ?? [])
            .Select(name => name.Trim())
            .Where(name => name.Length > 0)
            .Reverse()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Reverse()
            .ToList();
    }

    public PatcherConfiguration Copy()
    {
        Normalize();
        return new PatcherConfiguration
        {
            DisabledRecordTypes = new HashSet<string>(DisabledRecordTypes, StringComparer.Ordinal),
            IgnoredMods = new HashSet<string>(IgnoredMods, StringComparer.OrdinalIgnoreCase),
            AlwaysWinningMods = [.. AlwaysWinningMods],
            Forwarding = new ForwardingSettings
            {
                TreatCreationClubAsVanilla = Forwarding.TreatCreationClubAsVanilla,
                EnforceSingleVanillaWeaponTypeKeyword = Forwarding.EnforceSingleVanillaWeaponTypeKeyword,
                VanillaWeaponTypeKeywords = new HashSet<string>(
                    Forwarding.VanillaWeaponTypeKeywords,
                    StringComparer.OrdinalIgnoreCase),
                EditorIdPolicy = Forwarding.EditorIdPolicy,
                ProtectionPolicy = Forwarding.ProtectionPolicy
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
    private static readonly string[] DefaultVanillaWeaponTypeKeywordValues =
    [
        "06D932:Skyrim.esm", // WeapTypeBattleaxe
        "01E715:Skyrim.esm", // WeapTypeBow
        "01E713:Skyrim.esm", // WeapTypeDagger
        "06D931:Skyrim.esm", // WeapTypeGreatsword
        "01E714:Skyrim.esm", // WeapTypeMace
        "01E716:Skyrim.esm", // WeapTypeStaff
        "01E711:Skyrim.esm", // WeapTypeSword
        "01E712:Skyrim.esm", // WeapTypeWarAxe
        "06D930:Skyrim.esm"  // WeapTypeWarhammer
    ];

    public bool TreatCreationClubAsVanilla { get; set; } = true;

    /// <summary>
    /// When an override successfully introduces exactly one configured weapon type keyword,
    /// that override also owns removal of the other configured types. Overrides which
    /// explicitly contain multiple configured types are preserved as authored.
    /// </summary>
    public bool EnforceSingleVanillaWeaponTypeKeyword { get; set; } = true;

    public HashSet<string> VanillaWeaponTypeKeywords { get; set; } =
        new(DefaultVanillaWeaponTypeKeywordValues, StringComparer.OrdinalIgnoreCase);

    public EditorIdForwardingPolicy EditorIdPolicy { get; set; } =
        EditorIdForwardingPolicy.ForwardOnlyWithOtherChanges;

    public ProtectionForwardingPolicy ProtectionPolicy { get; set; } =
        ProtectionForwardingPolicy.PreferHigherWithAuthorizedDowngrades;

    public void Normalize()
    {
        VanillaWeaponTypeKeywords = new HashSet<string>(
            (VanillaWeaponTypeKeywords ?? [])
                .Select(value => value.Trim())
                .Where(value => value.Length > 0),
            StringComparer.OrdinalIgnoreCase);
    }
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
