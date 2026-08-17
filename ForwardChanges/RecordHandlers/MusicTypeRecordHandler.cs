using System;
using System.Collections.Generic;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.RecordHandlers;

// Migration note:
// - Generalized: MUSC scalar/flag/form-link-list fields and nested Data via reflection handlers.
// - Kept specialized: none.
// - Rationale: current IMusicType surface is fully representable with existing generic list/complex handlers.
public class MusicTypeRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "Flags", new SimpleReflectionFlagPropertyHandler<MusicType.Flag, IMusicType, IMusicTypeGetter>("Flags") },
        { "Data", new ComplexReflectionPropertyHandler<IMusicTypeDataGetter, IMusicType, IMusicTypeGetter>("Data") },
        { "FadeDuration", new SimpleReflectionPropertyHandler<float?, IMusicType, IMusicTypeGetter>("FadeDuration") },
        { "Tracks", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IMusicTrackGetter>, IMusicType, IMusicTypeGetter>("Tracks", ListOrdering.None, canBeNull: true) }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not IMusicTypeGetter musicTypeRecord)
        {
            throw new InvalidOperationException($"Expected IMusicTypeGetter but got {winningContext.Record.GetType()}");
        }

        return musicTypeRecord
            .ToLink<IMusicTypeGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IMusicType, IMusicTypeGetter>(state.LinkCache)
            .ToArray();
    }
}