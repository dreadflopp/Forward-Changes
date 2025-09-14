using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.PlacedNpc;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System;

namespace ForwardChanges.RecordHandlers
{
    public class PlacedNpcRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "MajorFlags", new MajorFlagsHandler() },
            { "Base", new BaseHandler() },
            { "EncounterZone", new EncounterZoneHandler() },
            { "Patrol", new PatrolHandler() },
            { "LevelModifier", new LevelModifierHandler() },
            { "MerchantContainer", new MerchantContainerHandler() },
            { "Count", new CountHandler() },
            { "Radius", new RadiusHandler() },
            { "Health", new HealthHandler() },
            { "LinkedReferences", new LinkedReferencesHandler() },
            { "ActivateParents", new ActivateParentsHandler() },
            { "LinkedReferenceColor", new LinkedReferenceColorHandler() },
            { "PersistentLocation", new PersistentLocationHandler() },
            { "LocationReference", new LocationReferenceHandler() },
            { "IsIgnoredBySandbox", new IsIgnoredBySandboxHandler() },
            { "LocationRefTypes", new LocationRefTypesHandler() },
            { "HeadTrackingWeight", new HeadTrackingWeightHandler() },
            { "Horse", new HorseHandler() },
            { "FavorCost", new FavorCostHandler() },
            { "EnableParent", new EnableParentHandler() },
            { "Owner", new OwnerHandler() },
            { "FactionRank", new FactionRankHandler() },
            { "Emittance", new EmittanceHandler() },
            { "MultiboundReference", new MultiboundReferenceHandler() },
            { "IsIgnoredBySandbox2", new IsIgnoredBySandbox2Handler() },
            { "Scale", new ScaleHandler() },
            { "Placement", new PlacementHandler() },
            { "VirtualMachineAdapter", new VirtualMachineAdapterHandler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IPlacedNpcGetter placedNpcRecord)
            {
                throw new InvalidOperationException($"Expected IPlacedNpcGetter but got {winningContext.Record.GetType()}");
            }
            var contexts = placedNpcRecord
                .ToLink<IPlacedNpcGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IPlacedNpc, IPlacedNpcGetter>(state.LinkCache)
                .ToArray();

            return contexts;
        }

        public override IMajorRecord GetOverrideRecord(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
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
                        handler.SetValue(record, value);
                    }
                    catch (Exception ex)
                    {
                        // Property doesn't exist on this placed npc type - just continue
                        Console.WriteLine($"Warning: Property {propertyName} not available on placed npc {record.FormKey}: {ex.Message}");
                    }
                }
            }
        }
    }
}