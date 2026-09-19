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
// - Generalized: RFCT art/shader links, flags, and metadata use shared semantic handlers.
// - Kept specialized: none.
// - Rationale: the record exposes only independent links and flag values.
public class VisualEffectRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "EffectArt", new SimpleReflectionFormLinkPropertyHandler<IArtObjectGetter, IVisualEffect, IVisualEffectGetter>("EffectArt") },
        { "Shader", new SimpleReflectionFormLinkPropertyHandler<IEffectShaderGetter, IVisualEffect, IVisualEffectGetter>("Shader") },
        { "Flags", new SimpleReflectionFlagPropertyHandler<VisualEffect.Flag, IVisualEffect, IVisualEffectGetter>("Flags") }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not IVisualEffectGetter visualEffectRecord)
        {
            throw new InvalidOperationException($"Expected IVisualEffectGetter but got {winningContext.Record.GetType()}");
        }

        return visualEffectRecord
            .ToLink<IVisualEffectGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IVisualEffect, IVisualEffectGetter>(state.LinkCache)
            .ToArray();
    }
}
