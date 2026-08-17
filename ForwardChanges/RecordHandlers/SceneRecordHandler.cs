using System;
using System.Collections.Generic;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers;

// Migration note:
// - Generalized: SCEN scalar, list, binary, link, and nested structures via generic handlers.
// - Kept specialized: none.
// - Rationale: mutable scene surface maps directly to existing reflection-based handlers.
public class SceneRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "VirtualMachineAdapter", new ComplexReflectionPropertyHandler<ISceneAdapterGetter, IScene, ISceneGetter>("VirtualMachineAdapter") },
        { "Flags", new SimpleReflectionPropertyHandler<Scene.Flag?, IScene, ISceneGetter>("Flags") },
        { "Phases", new SimpleReflectionListPropertyHandler<IScenePhaseGetter, IScene, ISceneGetter>("Phases", ListOrdering.None) },
        { "Actors", new SimpleReflectionListPropertyHandler<ISceneActorGetter, IScene, ISceneGetter>("Actors", ListOrdering.None) },
        { "Actions", new SimpleReflectionListPropertyHandler<ISceneActionGetter, IScene, ISceneGetter>("Actions", ListOrdering.None) },
        { "Unused", new ComplexReflectionPropertyHandler<IScenePhaseUnusedDataGetter, IScene, ISceneGetter>("Unused") },
        { "Unused2", new ComplexReflectionPropertyHandler<IScenePhaseUnusedDataGetter, IScene, ISceneGetter>("Unused2") },
        { "Quest", new SimpleReflectionFormLinkPropertyHandler<IQuestGetter, IScene, ISceneGetter>("Quest") },
        { "LastActionIndex", new SimpleReflectionPropertyHandler<uint?, IScene, ISceneGetter>("LastActionIndex") },
        { "VNAM", new SimpleReflectionBinaryDataPropertyHandler<IScene, ISceneGetter>("VNAM") },
        { "Conditions", new SimpleReflectionListPropertyHandler<IConditionGetter, IScene, ISceneGetter>("Conditions", ListOrdering.None) }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not ISceneGetter record)
        {
            throw new InvalidOperationException($"Expected ISceneGetter but got {winningContext.Record.GetType()}");
        }

        return record
            .ToLink<ISceneGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IScene, ISceneGetter>(state.LinkCache)
            .ToArray();
    }
}
