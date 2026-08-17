using System;
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
    // - Generalized: Parts list via reflection-based complex handler.
    // - Kept specialized: Model via shared model handler.
    // - Rationale: preserves modeled-record behavior while adding concrete BPTD support.
    public class BodyPartDataRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Model", new ModelHandler() },
            { "Parts", new ComplexReflectionPropertyHandler<IReadOnlyList<IBodyPartGetter>, IBodyPartData, IBodyPartDataGetter>("Parts") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IBodyPartDataGetter bodyPartData)
            {
                throw new InvalidOperationException($"Expected IBodyPartDataGetter but got {winningContext.Record.GetType()}");
            }

            return bodyPartData
                .ToLink<IBodyPartDataGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IBodyPartData, IBodyPartDataGetter>(state.LinkCache)
                .ToArray();
        }
    }
}
