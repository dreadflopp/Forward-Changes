using System;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.LeveledSpell;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using DreadsMashedPatch.RecordHandlers.Abstracts;
using Noggog;

namespace DreadsMashedPatch.RecordHandlers
{
    // Migration note:
    // - Generalized: entry owner copying/equality now uses Mutagen's OwnerTarget implementation, including UntypedOwner.
    // - Specialized: leveled-spell entry sorting/data comparison remains record-specific; Flags stays on the approved flag handler.
    // - Rationale: generated union behavior replaces obsolete raw reflection without changing list or flag policy.
    public class LeveledSpellRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "ObjectBounds", new ObjectBoundsHandler() },
            { "ChanceNone", new SimpleReflectionPropertyHandler<Percent, ILeveledSpell, ILeveledSpellGetter>("ChanceNone") },
            { "Flags", new SimpleReflectionFlagPropertyHandler<Mutagen.Bethesda.Skyrim.LeveledSpell.Flag, ILeveledSpell, ILeveledSpellGetter>("Flags") },
            { "Entries", new EntriesHandler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not ILeveledSpellGetter leveledSpell)
            {
                throw new InvalidOperationException($"Expected ILeveledSpellGetter but got {winningContext.Record.GetType()}");
            }

            return leveledSpell
                .ToLink<ILeveledSpellGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, ILeveledSpell, ILeveledSpellGetter>(state.LinkCache)
                .ToArray();
        }
    }
}
