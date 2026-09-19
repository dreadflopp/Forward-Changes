using System;
using System.Drawing;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.RecordHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.Interfaces;

namespace DreadsMashedPatch.RecordHandlers
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
            /* Conflict resolution for these is unnescessary
            { "PersistentActorReferencesAdded", new SimpleReflectionListPropertyHandler<IPersistentActorReferenceGetter, ILocation, ILocationGetter>("PersistentActorReferencesAdded", ListSemantics.SortedKeyed, canBeNull: true, keySelector: entry => entry.Actor.FormKey) },
            { "PersistentActorReferencesStatic", new SimpleReflectionListPropertyHandler<IPersistentActorReferenceGetter, ILocation, ILocationGetter>("PersistentActorReferencesStatic", ListSemantics.SortedKeyed, canBeNull: true, keySelector: entry => entry.Actor.FormKey) },
            { "PersistentActorReferencesRemoved", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IPlacedSimpleGetter>, ILocation, ILocationGetter>("PersistentActorReferencesRemoved", ListSemantics.SortedKeyed, canBeNull: true) },
            { "UniqueActorReferencesAdded", new SimpleReflectionListPropertyHandler<IUniqueActorReferenceGetter, ILocation, ILocationGetter>("UniqueActorReferencesAdded", ListSemantics.SortedKeyed, canBeNull: true, keySelector: entry => entry.Ref.FormKey) },
            { "UniqueActorReferencesStatic", new SimpleReflectionListPropertyHandler<IUniqueActorReferenceGetter, ILocation, ILocationGetter>("UniqueActorReferencesStatic", ListSemantics.SortedKeyed, canBeNull: true, keySelector: entry => entry.Ref.FormKey) },
            { "UniqueActorReferencesRemoved", new SimpleReflectionListPropertyHandler<IFormLinkGetter<INpcGetter>, ILocation, ILocationGetter>("UniqueActorReferencesRemoved", ListSemantics.SortedKeyed, canBeNull: true) },
            { "LocationRefTypeReferencesAdded", new SimpleReflectionListPropertyHandler<ILocationRefTypeReferenceGetter, ILocation, ILocationGetter>("LocationRefTypeReferencesAdded", ListSemantics.SortedKeyed, canBeNull: true, keySelector: entry => entry.Ref.FormKey) },
            { "LocationRefTypeReferencesStatic", new SimpleReflectionListPropertyHandler<ILocationRefTypeReferenceGetter, ILocation, ILocationGetter>("LocationRefTypeReferencesStatic", ListSemantics.SortedKeyed, canBeNull: true, keySelector: entry => entry.Ref.FormKey) },
            { "LocationRefTypeReferencesRemoved", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IPlacedSimpleGetter>, ILocation, ILocationGetter>("LocationRefTypeReferencesRemoved", ListSemantics.SortedKeyed, canBeNull: true) },
            { "WorldspaceCellsAdded", new SimpleReflectionListPropertyHandler<ILocationCoordinateGetter, ILocation, ILocationGetter>("WorldspaceCellsAdded", ListSemantics.SortedKeyed, keySelector: entry => entry.Location.FormKey) },
            { "WorldspaceCellsStatic", new SimpleReflectionListPropertyHandler<ILocationCoordinateGetter, ILocation, ILocationGetter>("WorldspaceCellsStatic", ListSemantics.SortedKeyed, keySelector: entry => entry.Location.FormKey) },
            { "WorldspaceCellsRemoved", new SimpleReflectionListPropertyHandler<ILocationCoordinateGetter, ILocation, ILocationGetter>("WorldspaceCellsRemoved", ListSemantics.SortedKeyed, keySelector: entry => entry.Location.FormKey) },
            { "InitiallyDisabledReferencesAdded", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IPlacedGetter>, ILocation, ILocationGetter>("InitiallyDisabledReferencesAdded", ListSemantics.SortedKeyed, canBeNull: true) },
            { "InitiallyDisabledReferencesStatic", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IPlacedGetter>, ILocation, ILocationGetter>("InitiallyDisabledReferencesStatic", ListSemantics.SortedKeyed, canBeNull: true) },
            { "EnableParentReferencesAdded", new SimpleReflectionListPropertyHandler<IEnableParentReferenceGetter, ILocation, ILocationGetter>("EnableParentReferencesAdded", ListSemantics.SortedKeyed, canBeNull: true, keySelector: entry => entry.Actor.FormKey) },
            { "EnableParentReferencesStatic", new SimpleReflectionListPropertyHandler<IEnableParentReferenceGetter, ILocation, ILocationGetter>("EnableParentReferencesStatic", ListSemantics.SortedKeyed, canBeNull: true, keySelector: entry => entry.Actor.FormKey) },
            */
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
