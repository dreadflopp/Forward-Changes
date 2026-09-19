using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace ForwardChanges;

public static class Utility
{
    private static readonly HashSet<ModKey> BaseGamePlugins =
    [
        ModKey.FromNameAndExtension("Skyrim.esm"),
        ModKey.FromNameAndExtension("Update.esm"),
        ModKey.FromNameAndExtension("Dawnguard.esm"),
        ModKey.FromNameAndExtension("HearthFires.esm"),
        ModKey.FromNameAndExtension("Dragonborn.esm"),
        ModKey.FromNameAndExtension("SkyrimVR.esm")
    ];

    private static HashSet<ModKey> _vanillaMods = new(BaseGamePlugins);

    public static bool IsVanilla(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context) =>
        IsVanilla(context.ModKey);

    public static bool IsVanilla(ModKey modKey) => _vanillaMods.Contains(modKey);

    public static VanillaBaselineSummary InitializeVanillaMods(
        IEnumerable<ModKey> loadOrder,
        IEnumerable<ModKey> creationClubPlugins,
        bool includeCreationClub)
    {
        ArgumentNullException.ThrowIfNull(loadOrder);
        ArgumentNullException.ThrowIfNull(creationClubPlugins);

        var presentMods = loadOrder.ToHashSet();
        var configuredBaseline = new HashSet<ModKey>(BaseGamePlugins);
        if (includeCreationClub)
        {
            configuredBaseline.UnionWith(creationClubPlugins);
        }

        var effectiveBaseline = new HashSet<ModKey>(configuredBaseline);
        effectiveBaseline.IntersectWith(presentMods);
        _vanillaMods = effectiveBaseline;

        return new VanillaBaselineSummary(
            effectiveBaseline.Count,
            configuredBaseline.Count - effectiveBaseline.Count,
            includeCreationClub);
    }
}

public sealed record VanillaBaselineSummary(int PresentCount, int MissingCount, bool IncludesCreationClub);
