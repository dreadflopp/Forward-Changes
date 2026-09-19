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
// - Generalized: MSTT fields including flags and looping sound via existing handlers.
// - Kept specialized: shared bounds/name/model handlers.
// - Rationale: follows Static-style forwarding while preserving moveable-static-specific fields.
public class MoveableStaticRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "ObjectBounds", new ObjectBoundsHandler() },
        { "Name", new NameHandler() },
        { "Model", new ModelHandler() },
        { "Destructible", new GeneratedCopyReflectionPropertyHandler<IDestructibleGetter, Destructible, IMoveableStatic, IMoveableStaticGetter>("Destructible", value => value.DeepCopy(), DestructibleMixIn.Equals) },
        { "Flags", new SimpleReflectionFlagPropertyHandler<MoveableStatic.Flag, IMoveableStatic, IMoveableStaticGetter>("Flags") },
        { "LoopingSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IMoveableStatic, IMoveableStaticGetter>("LoopingSound") },
        { "MajorFlags", new SimpleReflectionFlagPropertyHandler<MoveableStatic.MajorFlag, IMoveableStatic, IMoveableStaticGetter>("MajorFlags") }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not IMoveableStaticGetter moveableStaticRecord)
        {
            throw new InvalidOperationException($"Expected IMoveableStaticGetter but got {winningContext.Record.GetType()}");
        }

        return moveableStaticRecord
            .ToLink<IMoveableStaticGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IMoveableStatic, IMoveableStaticGetter>(state.LinkCache)
            .ToArray();
    }
}
