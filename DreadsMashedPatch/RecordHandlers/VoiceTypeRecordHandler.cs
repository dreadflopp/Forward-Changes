using System;
using System.Collections.Generic;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using DreadsMashedPatch.RecordHandlers.Abstracts;

namespace DreadsMashedPatch.RecordHandlers;

// Migration note:
// - Generalized: VTYP flags and metadata use shared semantic handlers.
// - Kept specialized: none; Flags remains on the project-approved flag handler path.
// - Rationale: the record exposes no coupled aggregate or list properties.
public class VoiceTypeRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "Flags", new SimpleReflectionFlagPropertyHandler<VoiceType.Flag, IVoiceType, IVoiceTypeGetter>("Flags") }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not IVoiceTypeGetter voiceTypeRecord)
        {
            throw new InvalidOperationException($"Expected IVoiceTypeGetter but got {winningContext.Record.GetType()}");
        }

        return voiceTypeRecord
            .ToLink<IVoiceTypeGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IVoiceType, IVoiceTypeGetter>(state.LinkCache)
            .ToArray();
    }
}
