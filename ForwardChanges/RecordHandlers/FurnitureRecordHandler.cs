using System;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Assets;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Furniture;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers.Abstracts;
using Noggog;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: VM/bounds/name/model, keywords, binary data, links, workbench/markers/model filename.
    // - Kept specialized: Destructible and nullable furniture flags via dedicated handlers.
    // - Rationale: preserve destructible and flag semantics while reusing stable shared handlers.
    public class FurnitureRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "VirtualMachineAdapter", new SimpleReflectionVirtualMachineAdapterHandler<IFurniture, IFurnitureGetter>() },
            { "ObjectBounds", new ObjectBoundsHandler() },
            { "Name", new NameHandler() },
            { "Model", new ModelHandler() },
            { "Destructible", new DestructibleHandler() },
            { "Keywords", new KeywordListHandler() },
            { "PNAM", new SimpleReflectionBinaryDataPropertyHandler<IFurniture, IFurnitureGetter>("PNAM") },
            { "Flags", new FlagsHandler() },
            { "InteractionKeyword", new SimpleReflectionFormLinkPropertyHandler<IKeywordGetter, IFurniture, IFurnitureGetter>("InteractionKeyword") },
            { "WorkbenchData", new ComplexReflectionPropertyHandler<IWorkbenchDataGetter, IFurniture, IFurnitureGetter>("WorkbenchData") },
            { "AssociatedSpell", new SimpleReflectionFormLinkPropertyHandler<ISpellGetter, IFurniture, IFurnitureGetter>("AssociatedSpell") },
            { "Markers", new SimpleReflectionListPropertyHandler<IFurnitureMarkerGetter, IFurniture, IFurnitureGetter>("Markers", ListOrdering.None, true) },
            { "ModelFilename", new SimpleReflectionPropertyHandler<AssetLinkGetter<SkyrimModelAssetType>?, IFurniture, IFurnitureGetter>("ModelFilename") },
            { "MajorFlags", new SimpleReflectionFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Furniture.MajorFlag, IFurniture, IFurnitureGetter>("MajorFlags") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IFurnitureGetter furniture)
            {
                throw new InvalidOperationException($"Expected IFurnitureGetter but got {winningContext.Record.GetType()}");
            }

            return furniture
                .ToLink<IFurnitureGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IFurniture, IFurnitureGetter>(state.LinkCache)
                .ToArray();
        }
    }
}
