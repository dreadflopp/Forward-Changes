using System;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.LeveledNpc;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using DreadsMashedPatch.RecordHandlers.Abstracts;
using Noggog;

namespace DreadsMashedPatch.RecordHandlers
{
    // Migration note:
    // - Generalized: entry owner copying/equality uses Mutagen's OwnerTarget implementation,
    //   and unordered list matching now preserves duplicate occurrence counts.
    // - Specialized: leveled-NPC entry sorting/data comparison remains record-specific; Flags stays on the approved flag handler.
    // - Rationale: entries are an unordered multiset, so equal values may legitimately occur more than once.
    public class LeveledNpcRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "ChanceNone", new SimpleReflectionPropertyHandler<Percent, ILeveledNpc, ILeveledNpcGetter>("ChanceNone") },
            { "Flags", new SimpleReflectionFlagPropertyHandler<Mutagen.Bethesda.Skyrim.LeveledNpc.Flag, ILeveledNpc, ILeveledNpcGetter>("Flags") },
            { "Global", new SimpleReflectionFormLinkPropertyHandler<IGlobalGetter, ILeveledNpc, ILeveledNpcGetter>("Global") },
            { "Entries", new EntriesHandler() },
            { "ModelAndBounds", new ModelBoundsHandler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not ILeveledNpcGetter leveledNpc)
            {
                throw new InvalidOperationException($"Expected ILeveledNpcGetter but got {winningContext.Record.GetType()}");
            }

            return leveledNpc
                .ToLink<ILeveledNpcGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, ILeveledNpc, ILeveledNpcGetter>(state.LinkCache)
                .ToArray();
        }
    }
}
