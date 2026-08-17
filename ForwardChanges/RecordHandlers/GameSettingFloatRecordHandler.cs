using System;
using System.Collections.Generic;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers;

// Migration note:
// - Generalized: GMST float variant via scalar reflection handlers.
// - Kept specialized: typed Data handling remains per concrete GMST variant.
// - Rationale: concrete Data type differs across GameSetting variants.
public class GameSettingFloatRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "Data", new SimpleReflectionPropertyHandler<float?, IGameSettingFloat, IGameSettingFloatGetter>("Data") }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not IGameSettingFloatGetter record)
        {
            throw new InvalidOperationException($"Expected IGameSettingFloatGetter but got {winningContext.Record.GetType()}");
        }

        return record
            .ToLink<IGameSettingFloatGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IGameSettingFloat, IGameSettingFloatGetter>(state.LinkCache)
            .ToArray();
    }
}
