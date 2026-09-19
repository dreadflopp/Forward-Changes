using System;
using System.Collections.Generic;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim.Assets;
using DreadsMashedPatch.RecordHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.MusicTrack;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.Interfaces;

namespace DreadsMashedPatch.RecordHandlers;

// Migration note:
// - Generalized: MUST scalar, aggregate, and serialized asset-path fields use shared semantic handlers.
// - Kept specialized: conditions retain CTDA alignment; cue points retain typed handling; Tracks is atomic.
// - Rationale: xEdit declares track order semantic without an entry key, so the sequence has one owner.
public class MusicTrackRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "Type", new SimpleReflectionPropertyHandler<MusicTrack.TypeEnum, IMusicTrack, IMusicTrackGetter>("Type") },
        { "Duration", new SimpleReflectionPropertyHandler<float?, IMusicTrack, IMusicTrackGetter>("Duration") },
        { "FadeOut", new SimpleReflectionPropertyHandler<float?, IMusicTrack, IMusicTrackGetter>("FadeOut") },
        { "TrackFilename", new TrackAssetLinkHandler("TrackFilename") },
        { "FinaleFilename", new TrackAssetLinkHandler("FinaleFilename") },
        { "LoopData", new ComplexReflectionPropertyHandler<IMusicTrackLoopDataGetter, IMusicTrack, IMusicTrackGetter>("LoopData") },
        { "CuePoints", new CuePointsHandler() },
        { "Conditions", new ConditionsHandler() },
        { "Tracks", new AtomicReflectionListPropertyHandler<IFormLinkGetter<IMusicTrackGetter>, IMusicTrack, IMusicTrackGetter>("Tracks") }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not IMusicTrackGetter musicTrackRecord)
        {
            throw new InvalidOperationException($"Expected IMusicTrackGetter but got {winningContext.Record.GetType()}");
        }

        return musicTrackRecord
            .ToLink<IMusicTrackGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IMusicTrack, IMusicTrackGetter>(state.LinkCache)
            .ToArray();
    }
}
