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
// - Generalized: TACT script/model/link/scalar fields and flags via existing handlers.
// - Kept specialized: shared bounds/name/keyword handlers.
// - Rationale: mirrors Activator-style forwarding with type-specific property names.
public class TalkingActivatorRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "VirtualMachineAdapter", new SimpleReflectionVirtualMachineAdapterHandler<ITalkingActivator, ITalkingActivatorGetter>() },
        { "ObjectBounds", new ObjectBoundsHandler() },
        { "Name", new NameHandler() },
        { "Model", new ModelHandler() },
        { "Destructible", new GeneratedCopyReflectionPropertyHandler<IDestructibleGetter, Destructible, ITalkingActivator, ITalkingActivatorGetter>("Destructible", value => value.DeepCopy(), DestructibleMixIn.Equals) },
        { "Keywords", new KeywordListHandler() },
        { "PNAM", new SimpleReflectionPropertyHandler<int?, ITalkingActivator, ITalkingActivatorGetter>("PNAM") },
        { "LoopingSound", new SimpleReflectionFormLinkPropertyHandler<ISoundMarkerGetter, ITalkingActivator, ITalkingActivatorGetter>("LoopingSound") },
        { "FNAM", new SimpleReflectionPropertyHandler<short?, ITalkingActivator, ITalkingActivatorGetter>("FNAM") },
        { "Voice", new SimpleReflectionFormLinkPropertyHandler<IVoiceTypeGetter, ITalkingActivator, ITalkingActivatorGetter>("Voice") },
        { "MajorFlags", new SimpleReflectionFlagPropertyHandler<TalkingActivator.MajorFlag, ITalkingActivator, ITalkingActivatorGetter>("MajorFlags") }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not ITalkingActivatorGetter talkingActivatorRecord)
        {
            throw new InvalidOperationException($"Expected ITalkingActivatorGetter but got {winningContext.Record.GetType()}");
        }

        return talkingActivatorRecord
            .ToLink<ITalkingActivatorGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, ITalkingActivator, ITalkingActivatorGetter>(state.LinkCache)
            .ToArray();
    }
}
