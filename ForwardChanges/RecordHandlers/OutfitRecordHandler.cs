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
// - Generalized: OTFT Items uses the shared sorted/keyed FormLink list implementation.
// - Kept specialized: none; header flags retain the established shared handlers.
// - Rationale: xEdit sorts outfit entries by FormID, so declaration order does not establish ownership.
public class OutfitRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "Items", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IOutfitTargetGetter>, IOutfit, IOutfitGetter>("Items", ListSemantics.SortedKeyed) }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not IOutfitGetter outfitRecord)
        {
            throw new InvalidOperationException($"Expected IOutfitGetter but got {winningContext.Record.GetType()}");
        }

        return outfitRecord
            .ToLink<IOutfitGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IOutfit, IOutfitGetter>(state.LinkCache)
            .ToArray();
    }
}
