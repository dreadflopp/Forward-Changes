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
// - Generalized: SOPM aggregates use Mutagen-generated copy/equality operations; top-level binary payloads use shared binary handlers.
// - Kept specialized: none; generated aggregate copies preserve overlay-only nested values such as attenuation byte slices and channels.
// - Intentionally non-migrated: none.
// - Rationale: generic reflection cannot safely convert nested ReadOnlyMemorySlice or overlay objects to their mutable counterparts.
public class SoundOutputModelRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "Data", new GeneratedCopyReflectionPropertyHandler<ISoundOutputDataGetter, SoundOutputData, ISoundOutputModel, ISoundOutputModelGetter>("Data", value => value.DeepCopy(), SoundOutputDataMixIn.Equals) },
        { "FNAM", new SimpleReflectionBinaryDataPropertyHandler<ISoundOutputModel, ISoundOutputModelGetter>("FNAM") },
        { "Type", new SimpleReflectionPropertyHandler<SoundOutputModel.TypeEnum?, ISoundOutputModel, ISoundOutputModelGetter>("Type") },
        { "CNAM", new SimpleReflectionBinaryDataPropertyHandler<ISoundOutputModel, ISoundOutputModelGetter>("CNAM") },
        { "SNAM", new SimpleReflectionBinaryDataPropertyHandler<ISoundOutputModel, ISoundOutputModelGetter>("SNAM") },
        { "OutputChannels", new GeneratedCopyReflectionPropertyHandler<ISoundOutputChannelsGetter, SoundOutputChannels, ISoundOutputModel, ISoundOutputModelGetter>("OutputChannels", value => value.DeepCopy(), SoundOutputChannelsMixIn.Equals) },
        { "Attenuation", new GeneratedCopyReflectionPropertyHandler<ISoundOutputAttenuationGetter, SoundOutputAttenuation, ISoundOutputModel, ISoundOutputModelGetter>("Attenuation", value => value.DeepCopy(), SoundOutputAttenuationMixIn.Equals) }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not ISoundOutputModelGetter soundOutputModelRecord)
        {
            throw new InvalidOperationException($"Expected ISoundOutputModelGetter but got {winningContext.Record.GetType()}");
        }

        return soundOutputModelRecord
            .ToLink<ISoundOutputModelGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, ISoundOutputModel, ISoundOutputModelGetter>(state.LinkCache)
            .ToArray();
    }
}
