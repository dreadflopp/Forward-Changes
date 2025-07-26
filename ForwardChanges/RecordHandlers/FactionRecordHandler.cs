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
            { "EditorID", new EditorIDPropertyHandler() },
            { "Name", new NamePropertyHandler() },
            { "Relations", new FactionRelationsListPropertyHandler() },
            { "Ranks", new FactionRanksListPropertyHandler() },
            { "Conditions", new FactionConditionsListPropertyHandler() },
            { "Flags", new FactionFlagsPropertyHandler() },
            { "ExteriorJailMarker", new FactionExteriorJailMarkerPropertyHandler() },
            { "FollowerWaitMarker", new FactionFollowerWaitMarkerPropertyHandler() },
            { "StolenGoodsContainer", new FactionStolenGoodsContainerPropertyHandler() },
            { "PlayerInventoryContainer", new FactionPlayerInventoryContainerPropertyHandler() },
            { "SharedCrimeFactionList", new FactionSharedCrimeFactionListPropertyHandler() },
            { "JailOutfit", new FactionJailOutfitPropertyHandler() },
            { "CrimeValues", new FactionCrimeValuesPropertyHandler() },
            { "VendorBuySellList", new FactionVendorBuySellListPropertyHandler() },
            { "MerchantContainer", new FactionMerchantContainerPropertyHandler() },
            { "VendorValues", new FactionVendorValuesPropertyHandler() },
            { "VendorLocation", new FactionVendorLocationPropertyHandler() }
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

        public override IMajorRecord GetOverrideRecord(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            // Use the context's GetOrAddAsOverride method like Fusion does
            return winningContext.GetOrAddAsOverride(state.PatchMod);
        }

        public override void ApplyForwardedProperties(IMajorRecord record, Dictionary<string, object?> propertiesToForward)
        {
            foreach (var (propertyName, value) in propertiesToForward)
            {
                if (PropertyHandlers.TryGetValue(propertyName, out var handler))
                {
                    try
                    {
                        Console.WriteLine($"[{propertyName}] Applying value: {handler.FormatValue(value)}, Type: {value?.GetType()}");
                        if (value != null)
                        {
                            handler.SetValue(record, value);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Property doesn't exist on this faction type - just continue
                        Console.WriteLine($"Warning: Property {propertyName} not available on faction {record.FormKey}: {ex.Message}");
                    }
                }
            }
        }


    }
}