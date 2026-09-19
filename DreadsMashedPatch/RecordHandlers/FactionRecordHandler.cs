using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using DreadsMashedPatch.PropertyHandlers.Faction;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.RecordHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using System;

namespace DreadsMashedPatch.RecordHandlers
{
    // Migration note:
    // - Generalized: Relations, Ranks, all non-flag FormLink properties, CrimeValues, VendorValues, VendorLocation.
    // - Kept specialized: Conditions (conditions-specific list semantics), Flags (project flag policy).
    // - Rationale: reflection handlers cover direct property forwarding; conditions and flags require project-specific behavior.
    public class FactionRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Name", new NameHandler() },
            { "Relations", new SimpleReflectionListPropertyHandler<IRelationGetter, IFaction, IFactionGetter>("Relations", ListSemantics.SortedKeyed, keySelector: relation => relation.Target.FormKey) },
            { "Ranks", new SimpleReflectionListPropertyHandler<IRankGetter, IFaction, IFactionGetter>("Ranks", ListSemantics.SortedKeyed, keySelector: rank => rank.Number) },
            { "Conditions", new ConditionsHandler() },
            { "Flags", new FlagsHandler() },
            { "ExteriorJailMarker", new SimpleReflectionFormLinkPropertyHandler<IPlacedObjectGetter, IFaction, IFactionGetter>("ExteriorJailMarker") },
            { "FollowerWaitMarker", new SimpleReflectionFormLinkPropertyHandler<IPlacedObjectGetter, IFaction, IFactionGetter>("FollowerWaitMarker") },
            { "StolenGoodsContainer", new SimpleReflectionFormLinkPropertyHandler<IPlacedObjectGetter, IFaction, IFactionGetter>("StolenGoodsContainer") },
            { "PlayerInventoryContainer", new SimpleReflectionFormLinkPropertyHandler<IPlacedObjectGetter, IFaction, IFactionGetter>("PlayerInventoryContainer") },
            { "SharedCrimeFactionList", new SimpleReflectionFormLinkPropertyHandler<IFormListGetter, IFaction, IFactionGetter>("SharedCrimeFactionList") },
            { "JailOutfit", new SimpleReflectionFormLinkPropertyHandler<IOutfitGetter, IFaction, IFactionGetter>("JailOutfit") },
            { "CrimeValues", new SimpleReflectionPropertyHandler<CrimeValues, IFaction, IFactionGetter>("CrimeValues") },
            { "VendorBuySellList", new SimpleReflectionFormLinkPropertyHandler<IFormListGetter, IFaction, IFactionGetter>("VendorBuySellList") },
            { "MerchantContainer", new SimpleReflectionFormLinkPropertyHandler<IPlacedObjectGetter, IFaction, IFactionGetter>("MerchantContainer") },
            { "VendorValues", new SimpleReflectionPropertyHandler<VendorValues, IFaction, IFactionGetter>("VendorValues") },
            { "VendorLocation", new SimpleReflectionPropertyHandler<LocationTargetRadius, IFaction, IFactionGetter>("VendorLocation") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IFactionGetter factionRecord)
            {
                throw new InvalidOperationException($"Expected IFactionGetter but got {winningContext.Record.GetType()}");
            }
            var contexts = factionRecord
                .ToLink<IFactionGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IFaction, IFactionGetter>(state.LinkCache)
                .ToArray();

            return contexts;
        }

        // GetOverrideRecord and ApplyForwardedProperties are now handled by the base class
        // The base class automatically handles flag property coordination


    }
}
