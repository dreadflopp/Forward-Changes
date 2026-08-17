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
    // - Generalized: Color and Playable via reflection handlers.
    // - Kept specialized: Name via shared translated-string handler.
    // - Rationale: very small record surface and fully reflection-safe fields.
    public class ColorRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Name", new NameHandler() },
            { "Color", new SimpleReflectionPropertyHandler<Color, IColorRecord, IColorRecordGetter>("Color") },
            { "Playable", new SimpleReflectionPropertyHandler<bool, IColorRecord, IColorRecordGetter>("Playable") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IColorRecordGetter colorRecord)
            {
                throw new InvalidOperationException($"Expected IColorRecordGetter but got {winningContext.Record.GetType()}");
            }

            return colorRecord
                .ToLink<IColorRecordGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IColorRecord, IColorRecordGetter>(state.LinkCache)
                .ToArray();
        }
    }
}
