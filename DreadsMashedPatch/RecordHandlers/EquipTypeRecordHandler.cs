using System;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.EquipType;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: UseAllParents via reflection handler.
    // - Kept specialized: SlotParents via dedicated list FormLink handler.
    // - Rationale: preserve nullable list semantics and concrete FormLink copy behavior.
    public class EquipTypeRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "SlotParents", new SlotParentsHandler() },
            { "UseAllParents", new SimpleReflectionPropertyHandler<bool?, IEquipType, IEquipTypeGetter>("UseAllParents") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IEquipTypeGetter equipType)
            {
                throw new InvalidOperationException($"Expected IEquipTypeGetter but got {winningContext.Record.GetType()}");
            }

            return equipType
                .ToLink<IEquipTypeGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IEquipType, IEquipTypeGetter>(state.LinkCache)
                .ToArray();
        }
    }
}
