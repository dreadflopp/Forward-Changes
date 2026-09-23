using DreadsMashedPatch.Contexts;
using DreadsMashedPatch.PropertyHandlers.General;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;

namespace DreadsMashedPatch.PropertyHandlers.Weapon;

/// <summary>
/// Applies ordinary keyword forwarding, then treats a newly accepted, singular vanilla
/// weapon type as superseding the other configured weapon types. Other keyword families
/// and explicitly authored multi-type weapon overrides retain normal list semantics.
/// </summary>
public sealed class WeaponKeywordListHandler : KeywordListHandler
{
    protected override void ProcessHandlerSpecificLogic(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
        ListPropertyContext<IFormLinkGetter<IKeywordGetter>> listPropertyContext,
        List<IFormLinkGetter<IKeywordGetter>> recordItems,
        List<ListPropertyValueContext<IFormLinkGetter<IKeywordGetter>>> currentForwardItems)
    {
        ApplyExclusiveWeaponTypeRule(
            context.ModKey.ToString(),
            PatcherSettings.EnforceSingleVanillaWeaponTypeKeyword,
            PatcherSettings.VanillaWeaponTypeKeywords,
            recordItems,
            currentForwardItems,
            removed => LogCollector.Add(
                PropertyName,
                $"[{PropertyName}] {context.ModKey}: Removing superseded vanilla weapon type {removed.Value.FormKey} " +
                $"(was owned by {removed.OwnerMod}, new owner: {context.ModKey}) Success"));
    }

    internal static void ApplyExclusiveWeaponTypeRule(
        string currentMod,
        bool enabled,
        IReadOnlySet<FormKey> configuredTypes,
        IReadOnlyCollection<IFormLinkGetter<IKeywordGetter>> declaredItems,
        List<ListPropertyValueContext<IFormLinkGetter<IKeywordGetter>>> forwardItems,
        Action<ListPropertyValueContext<IFormLinkGetter<IKeywordGetter>>>? beforeRemove = null)
    {
        if (!enabled || configuredTypes.Count < 2)
        {
            return;
        }

        var declaredTypes = declaredItems
            .Select(item => item.FormKey)
            .Where(configuredTypes.Contains)
            .Distinct()
            .ToArray();

        // Multiple types in one source override are presumed intentional. No configured
        // type means this override did not express a type choice.
        if (declaredTypes.Length != 1)
        {
            return;
        }

        var selectedType = declaredTypes[0];
        var acceptedAddition = forwardItems.Any(item =>
            !item.IsRemoved
            && item.Value.FormKey == selectedType
            && string.Equals(item.OwnerMod, currentMod, StringComparison.OrdinalIgnoreCase));
        if (!acceptedAddition)
        {
            return;
        }

        foreach (var item in forwardItems.Where(item =>
                     !item.IsRemoved
                     && item.Value.FormKey != selectedType
                     && configuredTypes.Contains(item.Value.FormKey)))
        {
            beforeRemove?.Invoke(item);
            item.IsRemoved = true;
            item.OwnerMod = currentMod;
        }
    }
}
