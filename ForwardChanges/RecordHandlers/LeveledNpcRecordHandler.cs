using System;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.LeveledNpc;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers.Abstracts;
using Noggog;

namespace ForwardChanges.RecordHandlers
{
    public class LeveledNpcRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "ObjectBounds", new ObjectBoundsHandler() },
            { "ChanceNone", new SimpleReflectionPropertyHandler<Percent, ILeveledNpc, ILeveledNpcGetter>("ChanceNone") },
            { "Flags", new SimpleReflectionFlagPropertyHandler<Mutagen.Bethesda.Skyrim.LeveledNpc.Flag, ILeveledNpc, ILeveledNpcGetter>("Flags") },
            { "Global", new SimpleReflectionFormLinkPropertyHandler<IGlobalGetter, ILeveledNpc, ILeveledNpcGetter>("Global") },
            { "Entries", new EntriesHandler() },
            { "Model", new ModelHandler() }
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