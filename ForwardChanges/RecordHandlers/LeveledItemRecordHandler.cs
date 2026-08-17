using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Noggog;
using ForwardChanges.PropertyHandlers.LeveledItem;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System;

namespace ForwardChanges.RecordHandlers
{
    public class LeveledItemRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "ObjectBounds", new ObjectBoundsHandler() },
            { "ChanceNone", new SimpleReflectionPropertyHandler<Percent, ILeveledItem, ILeveledItemGetter>("ChanceNone") },
            { "Flags", new FlagsHandler() },
            { "Global", new SimpleReflectionFormLinkPropertyHandler<IGlobalGetter, ILeveledItem, ILeveledItemGetter>("Global") },
            { "Entries", new EntriesHandler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not ILeveledItemGetter leveledItemRecord)
            {
                throw new InvalidOperationException($"Expected ILeveledItemGetter but got {winningContext.Record.GetType()}");
            }
            var contexts = leveledItemRecord
                .ToLink<ILeveledItemGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, ILeveledItem, ILeveledItemGetter>(state.LinkCache)
                .ToArray();

            return contexts;
        }

        // GetOverrideRecord and ApplyForwardedProperties are now handled by the base class
        // The base class automatically handles flag property coordination
    }
}
