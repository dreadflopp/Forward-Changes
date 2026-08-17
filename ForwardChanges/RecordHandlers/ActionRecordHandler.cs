using System;
using System.Drawing;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: Action color with reflection, plus core record flags/editor id.
    // - Kept specialized: none.
    // - Rationale: IActionRecord surface is small and reflection-safe.
    public class ActionRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Color", new SimpleReflectionPropertyHandler<Color?, IActionRecord, IActionRecordGetter>("Color") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IActionRecordGetter actionRecord)
            {
                throw new InvalidOperationException($"Expected IActionRecordGetter but got {winningContext.Record.GetType()}");
            }

            return actionRecord
                .ToLink<IActionRecordGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IActionRecord, IActionRecordGetter>(state.LinkCache)
                .ToArray();
        }
    }
}
