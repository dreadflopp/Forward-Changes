using System;
using System.Drawing;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.RecordHandlers
{
    public class LocationRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Name", new NameHandler() },
            { "Keywords", new KeywordListHandler() },
            { "ActorCellPersistentReferences", new SimpleReflectionListPropertyHandler<ILocationReferenceGetter, ILocation, ILocationGetter>("ActorCellPersistentReferences") },
            { "LocationCellPersistentReferences", new SimpleReflectionListPropertyHandler<ILocationReferenceGetter, ILocation, ILocationGetter>("LocationCellPersistentReferences") },
            { "ReferenceCellPersistentReferences", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IPlacedSimpleGetter>, ILocation, ILocationGetter>("ReferenceCellPersistentReferences") },
            { "ActorCellUniques", new SimpleReflectionListPropertyHandler<ILocationCellUniqueGetter, ILocation, ILocationGetter>("ActorCellUniques") },
            { "LocationCellUniques", new SimpleReflectionListPropertyHandler<ILocationCellUniqueGetter, ILocation, ILocationGetter>("LocationCellUniques") },
            { "ReferenceCellUnique", new SimpleReflectionListPropertyHandler<IFormLinkGetter<INpcGetter>, ILocation, ILocationGetter>("ReferenceCellUnique") },
            { "ActorCellStaticReferences", new SimpleReflectionListPropertyHandler<ILocationCellStaticReferenceGetter, ILocation, ILocationGetter>("ActorCellStaticReferences") },
            { "LocationCellStaticReferences", new SimpleReflectionListPropertyHandler<ILocationCellStaticReferenceGetter, ILocation, ILocationGetter>("LocationCellStaticReferences") },
            { "ReferenceCellStaticReferences", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IPlacedSimpleGetter>, ILocation, ILocationGetter>("ReferenceCellStaticReferences") },
            { "ActorCellEncounterCell", new SimpleReflectionListPropertyHandler<ILocationCoordinateGetter, ILocation, ILocationGetter>("ActorCellEncounterCell") },
            { "LocationCellEncounterCell", new SimpleReflectionListPropertyHandler<ILocationCoordinateGetter, ILocation, ILocationGetter>("LocationCellEncounterCell") },
            { "ReferenceCellEncounterCell", new SimpleReflectionListPropertyHandler<ILocationCoordinateGetter, ILocation, ILocationGetter>("ReferenceCellEncounterCell") },
            { "ActorCellMarkerReference", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IPlacedGetter>, ILocation, ILocationGetter>("ActorCellMarkerReference") },
            { "LocationCellMarkerReference", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IPlacedGetter>, ILocation, ILocationGetter>("LocationCellMarkerReference") },
            { "ActorCellEnablePoint", new SimpleReflectionListPropertyHandler<ILocationCellEnablePointGetter, ILocation, ILocationGetter>("ActorCellEnablePoint") },
            { "LocationCellEnablePoint", new SimpleReflectionListPropertyHandler<ILocationCellEnablePointGetter, ILocation, ILocationGetter>("LocationCellEnablePoint") },
            { "ParentLocation", new SimpleReflectionFormLinkPropertyHandler<ILocationGetter, ILocation, ILocationGetter>("ParentLocation") },
            { "Music", new SimpleReflectionFormLinkPropertyHandler<IMusicTypeGetter, ILocation, ILocationGetter>("Music") },
            { "UnreportedCrimeFaction", new SimpleReflectionFormLinkPropertyHandler<IFactionGetter, ILocation, ILocationGetter>("UnreportedCrimeFaction") },
            { "WorldLocationMarkerRef", new SimpleReflectionFormLinkPropertyHandler<IPlacedSimpleGetter, ILocation, ILocationGetter>("WorldLocationMarkerRef") },
            { "WorldLocationRadius", new SimpleReflectionPropertyHandler<float?, ILocation, ILocationGetter>("WorldLocationRadius") },
            { "HorseMarkerRef", new SimpleReflectionFormLinkPropertyHandler<IPlacedObjectGetter, ILocation, ILocationGetter>("HorseMarkerRef") },
            { "Color", new SimpleReflectionPropertyHandler<Color?, ILocation, ILocationGetter>("Color") }
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
    }
}