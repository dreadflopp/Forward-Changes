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
// - Generalized: GLOB int variant via scalar reflection handlers.
// - Kept specialized: typed Data handling remains per concrete GLOB variant.
// - Rationale: concrete Data type differs across Global variants.
public class GlobalIntRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "MajorFlags", new SimpleReflectionFlagPropertyHandler<Global.MajorFlag, IGlobalInt, IGlobalIntGetter>("MajorFlags") },
        { "Data", new SimpleReflectionPropertyHandler<int?, IGlobalInt, IGlobalIntGetter>("Data") }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not IGlobalIntGetter record)
        {
            throw new InvalidOperationException($"Expected IGlobalIntGetter but got {winningContext.Record.GetType()}");
        }

        return record
            .ToLink<IGlobalIntGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IGlobalInt, IGlobalIntGetter>(state.LinkCache)
            .ToArray();
    }
}
