using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Location;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System;

// NOTE: The handler is complete
namespace ForwardChanges.RecordHandlers
{
    public class LocationRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Name", new NameHandler() },
            { "Keywords", new KeywordListHandler() },
            { "ActorCellPersistentReferences", new ActorCellPersistentReferencesHandler() },
            { "LocationCellPersistentReferences", new LocationCellPersistentReferencesHandler() },
            { "ReferenceCellPersistentReferences", new ReferenceCellPersistentReferencesHandler() },
            { "ActorCellUniques", new ActorCellUniquesHandler() },
            { "LocationCellUniques", new LocationCellUniquesHandler() },
            { "ReferenceCellUnique", new ReferenceCellUniqueHandler() },
            { "ActorCellStaticReferences", new ActorCellStaticReferencesHandler() },
            { "LocationCellStaticReferences", new LocationCellStaticReferencesHandler() },
            { "ReferenceCellStaticReferences", new ReferenceCellStaticReferencesHandler() },
            { "ActorCellEncounterCell", new ActorCellEncounterCellHandler() },
            { "LocationCellEncounterCell", new LocationCellEncounterCellHandler() },
            { "ReferenceCellEncounterCell", new ReferenceCellEncounterCellHandler() },
            { "ActorCellMarkerReference", new ActorCellMarkerReferenceHandler() },
            { "LocationCellMarkerReference", new LocationCellMarkerReferenceHandler() },
            { "ActorCellEnablePoint", new ActorCellEnablePointHandler() },
            { "LocationCellEnablePoint", new LocationCellEnablePointHandler() },
            { "ParentLocation", new ParentLocationHandler() },
            { "Music", new MusicHandler() },
            { "UnreportedCrimeFaction", new UnreportedCrimeFactionHandler() },
            { "WorldLocationMarkerRef", new WorldLocationMarkerRefHandler() },
            { "WorldLocationRadius", new WorldLocationRadiusHandler() },
            { "HorseMarkerRef", new HorseMarkerRefHandler() },
            { "Color", new ColorHandler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not ILocationGetter locationRecord)
            {
                throw new InvalidOperationException($"Expected ILocationGetter but got {winningContext.Record.GetType()}");
            }
            var contexts = locationRecord
                .ToLink<ILocationGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, ILocation, ILocationGetter>(state.LinkCache)
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
                        // Property doesn't exist on this location type - just continue
                        Console.WriteLine($"Warning: Property {propertyName} not available on location {record.FormKey}: {ex.Message}");
                    }
                }
            }
        }
    }
}