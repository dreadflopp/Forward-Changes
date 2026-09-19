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
// - Generalized: GLOB unknown variant via scalar reflection handlers.
// - Kept specialized: typed Data/TypeChar handling remains per concrete GLOB variant.
// - Rationale: concrete Data type and TypeChar semantics differ across Global variants.
public class GlobalUnknownRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "TypeChar", new SimpleReflectionPropertyHandler<char, IGlobalUnknown, IGlobalUnknownGetter>("TypeChar") },
        { "MajorFlags", new SimpleReflectionFlagPropertyHandler<Global.MajorFlag, IGlobalUnknown, IGlobalUnknownGetter>("MajorFlags") },
        { "Data", new SimpleReflectionPropertyHandler<float?, IGlobalUnknown, IGlobalUnknownGetter>("Data") }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not IGlobalUnknownGetter record)
        {
            throw new InvalidOperationException($"Expected IGlobalUnknownGetter but got {winningContext.Record.GetType()}");
        }

        return record
            .ToLink<IGlobalUnknownGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IGlobalUnknown, IGlobalUnknownGetter>(state.LinkCache)
            .ToArray();
    }
}
