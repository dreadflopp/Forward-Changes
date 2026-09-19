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
// - Generalized: TREE fields via reflection-based handlers and existing shared property handlers.
// - Kept specialized: none.
// - Intentionally excluded: Unknown is outside the semantic conflict surface.
// - Rationale: surface aligns with existing flora/static forwarding patterns.
public class TreeRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "VirtualMachineAdapter", new SimpleReflectionVirtualMachineAdapterHandler<ITree, ITreeGetter>() },
        { "ObjectBounds", new ObjectBoundsHandler() },
        { "Model", new ModelHandler() },
        { "Ingredient", new SimpleReflectionFormLinkPropertyHandler<IHarvestTargetGetter, ITree, ITreeGetter>("Ingredient") },
        { "HarvestSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, ITree, ITreeGetter>("HarvestSound") },
        { "Production", new ComplexReflectionPropertyHandler<ISeasonalIngredientProductionGetter, ITree, ITreeGetter>("Production") },
        { "Name", new NameHandler() },
        { "TrunkFlexibility", new SimpleReflectionPropertyHandler<float, ITree, ITreeGetter>("TrunkFlexibility") },
        { "BranchFlexibility", new SimpleReflectionPropertyHandler<float, ITree, ITreeGetter>("BranchFlexibility") },
        { "LeafAmplitude", new SimpleReflectionPropertyHandler<float, ITree, ITreeGetter>("LeafAmplitude") },
        { "LeafFrequency", new SimpleReflectionPropertyHandler<float, ITree, ITreeGetter>("LeafFrequency") },
        { "MajorFlags", new SimpleReflectionFlagPropertyHandler<Tree.MajorFlag, ITree, ITreeGetter>("MajorFlags") }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not ITreeGetter treeRecord)
        {
            throw new InvalidOperationException($"Expected ITreeGetter but got {winningContext.Record.GetType()}");
        }

        return treeRecord
            .ToLink<ITreeGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, ITree, ITreeGetter>(state.LinkCache)
            .ToArray();
    }
}
