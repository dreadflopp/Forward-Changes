using System;
using System.Collections.Generic;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using DreadsMashedPatch.RecordHandlers.Abstracts;

namespace DreadsMashedPatch.RecordHandlers;

// Migration note:
// - Generalized: GLOB short variant via scalar reflection handlers.
// - Kept specialized: typed Data handling remains per concrete GLOB variant.
// - Rationale: concrete Data type differs across Global variants.
public class GlobalShortRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "MajorFlags", new SimpleReflectionFlagPropertyHandler<Global.MajorFlag, IGlobalShort, IGlobalShortGetter>("MajorFlags") },
        { "Data", new SimpleReflectionPropertyHandler<short?, IGlobalShort, IGlobalShortGetter>("Data") }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not IGlobalShortGetter record)
        {
            throw new InvalidOperationException($"Expected IGlobalShortGetter but got {winningContext.Record.GetType()}");
        }

        return record
            .ToLink<IGlobalShortGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IGlobalShort, IGlobalShortGetter>(state.LinkCache)
            .ToArray();
    }
}
