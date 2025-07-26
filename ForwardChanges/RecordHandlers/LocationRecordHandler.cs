using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers;
using ForwardChanges.PropertyHandlers.ListPropertyHandlers;
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
            { "EditorID", new EditorIDPropertyHandler() },
            { "Name", new NamePropertyHandler() },
            { "Keywords", new KeywordListPropertyHandler() },
            { "ActorCellPersistentReferences", new LocationActorCellPersistentReferencesListPropertyHandler() },
            { "LocationCellPersistentReferences", new LocationLocationCellPersistentReferencesListPropertyHandler() },
            { "ReferenceCellPersistentReferences", new LocationReferenceCellPersistentReferencesListPropertyHandler() },
            { "ActorCellUniques", new LocationActorCellUniquesListPropertyHandler() },
            { "LocationCellUniques", new LocationLocationCellUniquesListPropertyHandler() },
            { "ReferenceCellUnique", new LocationReferenceCellUniqueListPropertyHandler() },
            { "ActorCellStaticReferences", new LocationActorCellStaticReferencesListPropertyHandler() },
            { "LocationCellStaticReferences", new LocationLocationCellStaticReferencesListPropertyHandler() },
            { "ReferenceCellStaticReferences", new LocationReferenceCellStaticReferencesListPropertyHandler() },
            { "ActorCellEncounterCell", new LocationActorCellEncounterCellListPropertyHandler() },
            { "LocationCellEncounterCell", new LocationLocationCellEncounterCellListPropertyHandler() },
            { "ReferenceCellEncounterCell", new LocationReferenceCellEncounterCellListPropertyHandler() },
            { "ActorCellMarkerReference", new LocationActorCellMarkerReferenceListPropertyHandler() },
            { "LocationCellMarkerReference", new LocationLocationCellMarkerReferenceListPropertyHandler() },
            { "ActorCellEnablePoint", new LocationActorCellEnablePointListPropertyHandler() },
            { "LocationCellEnablePoint", new LocationLocationCellEnablePointListPropertyHandler() },
            { "ParentLocation", new LocationParentLocationPropertyHandler() },
            { "Music", new LocationMusicPropertyHandler() },
            { "UnreportedCrimeFaction", new LocationUnreportedCrimeFactionPropertyHandler() },
            { "WorldLocationMarkerRef", new LocationWorldLocationMarkerRefPropertyHandler() },
            { "WorldLocationRadius", new LocationWorldLocationRadiusPropertyHandler() },
            { "HorseMarkerRef", new LocationHorseMarkerRefPropertyHandler() },
            { "Color", new LocationColorPropertyHandler() }
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
                        // Property doesn't exist on this location type - just continue
                        Console.WriteLine($"Warning: Property {propertyName} not available on location {record.FormKey}: {ex.Message}");
                    }
                }
            }
        }
    }
}