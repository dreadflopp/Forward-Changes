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
    // Migration note:
    // - Generalized: Mutagen 0.54.4 Location Added/Static/Removed collections use the shared reflection list handlers.
    // - Specialized: none; flags, names, keywords, links, and scalar values retain their project-approved shared handlers.
    // - Rationale: only Mutagen's renamed collection surface changed; existing handler behavior remains appropriate.
    public class LocationRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Name", new NameHandler() },
            { "Keywords", new KeywordListHandler() },
            { "PersistentActorReferencesAdded", new SimpleReflectionListPropertyHandler<IPersistentActorReferenceGetter, ILocation, ILocationGetter>("PersistentActorReferencesAdded", canBeNull: true) },
            { "PersistentActorReferencesStatic", new SimpleReflectionListPropertyHandler<IPersistentActorReferenceGetter, ILocation, ILocationGetter>("PersistentActorReferencesStatic", canBeNull: true) },
            { "PersistentActorReferencesRemoved", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IPlacedSimpleGetter>, ILocation, ILocationGetter>("PersistentActorReferencesRemoved", canBeNull: true) },
            { "UniqueActorReferencesAdded", new SimpleReflectionListPropertyHandler<IUniqueActorReferenceGetter, ILocation, ILocationGetter>("UniqueActorReferencesAdded", canBeNull: true) },
            { "UniqueActorReferencesStatic", new SimpleReflectionListPropertyHandler<IUniqueActorReferenceGetter, ILocation, ILocationGetter>("UniqueActorReferencesStatic", canBeNull: true) },
            { "UniqueActorReferencesRemoved", new SimpleReflectionListPropertyHandler<IFormLinkGetter<INpcGetter>, ILocation, ILocationGetter>("UniqueActorReferencesRemoved", canBeNull: true) },
            { "LocationRefTypeReferencesAdded", new SimpleReflectionListPropertyHandler<ILocationRefTypeReferenceGetter, ILocation, ILocationGetter>("LocationRefTypeReferencesAdded", canBeNull: true) },
            { "LocationRefTypeReferencesStatic", new SimpleReflectionListPropertyHandler<ILocationRefTypeReferenceGetter, ILocation, ILocationGetter>("LocationRefTypeReferencesStatic", canBeNull: true) },
            { "LocationRefTypeReferencesRemoved", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IPlacedSimpleGetter>, ILocation, ILocationGetter>("LocationRefTypeReferencesRemoved", canBeNull: true) },
            { "WorldspaceCellsAdded", new SimpleReflectionListPropertyHandler<ILocationCoordinateGetter, ILocation, ILocationGetter>("WorldspaceCellsAdded") },
            { "WorldspaceCellsStatic", new SimpleReflectionListPropertyHandler<ILocationCoordinateGetter, ILocation, ILocationGetter>("WorldspaceCellsStatic") },
            { "WorldspaceCellsRemoved", new SimpleReflectionListPropertyHandler<ILocationCoordinateGetter, ILocation, ILocationGetter>("WorldspaceCellsRemoved") },
            { "InitiallyDisabledReferencesAdded", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IPlacedGetter>, ILocation, ILocationGetter>("InitiallyDisabledReferencesAdded", canBeNull: true) },
            { "InitiallyDisabledReferencesStatic", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IPlacedGetter>, ILocation, ILocationGetter>("InitiallyDisabledReferencesStatic", canBeNull: true) },
            { "EnableParentReferencesAdded", new SimpleReflectionListPropertyHandler<IEnableParentReferenceGetter, ILocation, ILocationGetter>("EnableParentReferencesAdded", canBeNull: true) },
            { "EnableParentReferencesStatic", new SimpleReflectionListPropertyHandler<IEnableParentReferenceGetter, ILocation, ILocationGetter>("EnableParentReferencesStatic", canBeNull: true) },
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
