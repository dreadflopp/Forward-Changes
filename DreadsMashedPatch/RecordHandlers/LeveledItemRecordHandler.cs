using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Noggog;
using DreadsMashedPatch.PropertyHandlers.LeveledItem;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.RecordHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using System;

namespace DreadsMashedPatch.RecordHandlers
{
    // Migration note:
    // - Generalized: entry owner copying/equality now uses Mutagen's OwnerTarget implementation, including UntypedOwner.
    // - Specialized: leveled-item entry sorting and data comparison remain record-specific; Flags stays on the approved flag handler.
    // - Rationale: generated union behavior replaces obsolete raw reflection without changing list or flag policy.
    public class LeveledItemRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "ObjectBounds", new ObjectBoundsHandler() },
            { "ChanceNone", new SimpleReflectionPropertyHandler<Percent, ILeveledItem, ILeveledItemGetter>("ChanceNone") },
            { "Flags", new FlagsHandler() },
            { "Global", new SimpleReflectionFormLinkPropertyHandler<IGlobalGetter, ILeveledItem, ILeveledItemGetter>("Global") },
            { "Entries", new EntriesHandler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not ILeveledItemGetter leveledItemRecord)
            {
                throw new InvalidOperationException($"Expected ILeveledItemGetter but got {winningContext.Record.GetType()}");
            }
            var contexts = leveledItemRecord
                .ToLink<ILeveledItemGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, ILeveledItem, ILeveledItemGetter>(state.LinkCache)
                .ToArray();

            return contexts;
        }

        // GetOverrideRecord and ApplyForwardedProperties are now handled by the base class
        // The base class automatically handles flag property coordination
    }
}
