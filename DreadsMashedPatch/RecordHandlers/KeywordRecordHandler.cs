using System;
using System.Drawing;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using DreadsMashedPatch.RecordHandlers.Abstracts;

namespace DreadsMashedPatch.RecordHandlers
{
    // Migration note:
    // - Generalized: keyword color and core record metadata via reflection handlers.
    // - Kept specialized: none.
    // - Rationale: the keyword surface is minimal and reflection-safe.
    public class KeywordRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Color", new SimpleReflectionPropertyHandler<Color?, IKeyword, IKeywordGetter>("Color") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IKeywordGetter keywordRecord)
            {
                throw new InvalidOperationException($"Expected IKeywordGetter but got {winningContext.Record.GetType()}");
            }

            return keywordRecord
                .ToLink<IKeywordGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IKeyword, IKeywordGetter>(state.LinkCache)
                .ToArray();
        }
    }
}