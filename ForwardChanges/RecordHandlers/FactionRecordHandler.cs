using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Faction;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System;

namespace ForwardChanges.RecordHandlers
{
    // NOTE: The handler is complete
    public class FactionRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Name", new NameHandler() },
            { "Relations", new RelationsHandler() },
            { "Ranks", new RanksHandler() },
            { "Conditions", new ConditionsHandler() },
            { "Flags", new FlagsHandler() },
            { "ExteriorJailMarker", new ExteriorJailMarkerHandler() },
            { "FollowerWaitMarker", new FollowerWaitMarkerHandler() },
            { "StolenGoodsContainer", new StolenGoodsContainerHandler() },
            { "PlayerInventoryContainer", new PlayerInventoryContainerHandler() },
            { "SharedCrimeFactionList", new SharedCrimeFactionHandler() },
            { "JailOutfit", new JailOutfitHandler() },
            { "CrimeValues", new CrimeValuesHandler() },
            { "VendorBuySellList", new VendorBuySellHandler() },
            { "MerchantContainer", new MerchantContainerHandler() },
            { "VendorValues", new VendorValuesHandler() },
            { "VendorLocation", new VendorLocationHandler() }
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