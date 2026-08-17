using System;
using System.Collections.Generic;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers;

// Migration note:
// - Generalized: SOPM complex subobjects and binary payload fields via reflection handlers.
// - Kept specialized: none.
// - Rationale: Mutagen surface is nullable complex structures plus raw byte slices, all handled generically.
public class SoundOutputModelRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "Data", new ComplexReflectionPropertyHandler<ISoundOutputDataGetter, ISoundOutputModel, ISoundOutputModelGetter>("Data") },
        { "FNAM", new SimpleReflectionBinaryDataPropertyHandler<ISoundOutputModel, ISoundOutputModelGetter>("FNAM") },
        { "Type", new SimpleReflectionPropertyHandler<SoundOutputModel.TypeEnum?, ISoundOutputModel, ISoundOutputModelGetter>("Type") },
        { "CNAM", new SimpleReflectionBinaryDataPropertyHandler<ISoundOutputModel, ISoundOutputModelGetter>("CNAM") },
        { "SNAM", new SimpleReflectionBinaryDataPropertyHandler<ISoundOutputModel, ISoundOutputModelGetter>("SNAM") },
        { "OutputChannels", new ComplexReflectionPropertyHandler<ISoundOutputChannelsGetter, ISoundOutputModel, ISoundOutputModelGetter>("OutputChannels") },
        { "Attenuation", new ComplexReflectionPropertyHandler<ISoundOutputAttenuationGetter, ISoundOutputModel, ISoundOutputModelGetter>("Attenuation") }
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