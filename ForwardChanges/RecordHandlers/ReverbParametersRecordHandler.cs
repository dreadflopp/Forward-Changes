using System;
using System.Collections.Generic;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Noggog;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers;

public class ReverbParametersRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "DecayMilliseconds", new SimpleReflectionPropertyHandler<ushort, IReverbParameters, IReverbParametersGetter>("DecayMilliseconds") },
        { "HfReferenceHertz", new SimpleReflectionPropertyHandler<ushort, IReverbParameters, IReverbParametersGetter>("HfReferenceHertz") },
        { "RoomFilter", new SimpleReflectionPropertyHandler<sbyte, IReverbParameters, IReverbParametersGetter>("RoomFilter") },
        { "RoomHfFilter", new SimpleReflectionPropertyHandler<sbyte, IReverbParameters, IReverbParametersGetter>("RoomHfFilter") },
        { "Reflections", new SimpleReflectionPropertyHandler<sbyte, IReverbParameters, IReverbParametersGetter>("Reflections") },
        { "ReverbAmp", new SimpleReflectionPropertyHandler<sbyte, IReverbParameters, IReverbParametersGetter>("ReverbAmp") },
        { "DecayHfRatio", new SimpleReflectionPropertyHandler<float, IReverbParameters, IReverbParametersGetter>("DecayHfRatio") },
        { "ReflectDelayMS", new SimpleReflectionPropertyHandler<byte, IReverbParameters, IReverbParametersGetter>("ReflectDelayMS") },
        { "ReverbDelayMS", new SimpleReflectionPropertyHandler<byte, IReverbParameters, IReverbParametersGetter>("ReverbDelayMS") },
        { "DiffusionPercent", new SimpleReflectionPropertyHandler<Percent, IReverbParameters, IReverbParametersGetter>("DiffusionPercent") },
        { "DensityPercent", new SimpleReflectionPropertyHandler<Percent, IReverbParameters, IReverbParametersGetter>("DensityPercent") },
        { "Unknown", new SimpleReflectionPropertyHandler<byte, IReverbParameters, IReverbParametersGetter>("Unknown") }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not IReverbParametersGetter reverbParametersRecord)
        {
            throw new InvalidOperationException($"Expected IReverbParametersGetter but got {winningContext.Record.GetType()}");
        }

        return reverbParametersRecord
            .ToLink<IReverbParametersGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IReverbParameters, IReverbParametersGetter>(state.LinkCache)
            .ToArray();
    }
}