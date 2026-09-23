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
// - Generalized: SLGM item fields, soul-capacity enums, and link fields via existing handlers.
// - Kept specialized: Name/ObjectBounds/Model/Value/Weight via existing project handlers.
// - Rationale: follows established misc-item forwarding pattern while preserving shared behavior.
public class SoulGemRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "Name", new NameHandler() },
        { "ModelAndBounds", new ModelBoundsHandler() },
        { "Icons", new SimpleReflectionIconsPropertyHandler<ISoulGem, ISoulGemGetter>("Icons") },
        { "Destructible", new GeneratedCopyReflectionPropertyHandler<IDestructibleGetter, Destructible, ISoulGem, ISoulGemGetter>("Destructible", value => value.DeepCopy(), DestructibleMixIn.Equals) },
        { "PickUpSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, ISoulGem, ISoulGemGetter>("PickUpSound") },
        { "PutDownSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, ISoulGem, ISoulGemGetter>("PutDownSound") },
        { "Keywords", new KeywordListHandler() },
        { "Value", new ValueHandler() },
        { "Weight", new WeightHandler() },
        { "ContainedSoul", new SimpleReflectionPropertyHandler<SoulGem.Level, ISoulGem, ISoulGemGetter>("ContainedSoul") },
        { "MaximumCapacity", new SimpleReflectionPropertyHandler<SoulGem.Level, ISoulGem, ISoulGemGetter>("MaximumCapacity") },
        { "LinkedTo", new SimpleReflectionFormLinkPropertyHandler<ISoulGemGetter, ISoulGem, ISoulGemGetter>("LinkedTo") },
        { "MajorFlags", new SimpleReflectionFlagPropertyHandler<SoulGem.MajorFlag, ISoulGem, ISoulGemGetter>("MajorFlags") }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not ISoulGemGetter soulGemRecord)
        {
            throw new InvalidOperationException($"Expected ISoulGemGetter but got {winningContext.Record.GetType()}");
        }

        return soulGemRecord
            .ToLink<ISoulGemGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, ISoulGem, ISoulGemGetter>(state.LinkCache)
            .ToArray();
    }
}
