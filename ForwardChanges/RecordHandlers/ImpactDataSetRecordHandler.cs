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
    // - Generalized: the Impacts list via reflection list handling.
    // - Kept specialized: none.
    // - Rationale: the record is just a container for ImpactData list entries.
    public class ImpactDataSetRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Impacts", new SimpleReflectionListPropertyHandler<IImpactDataGetter, IImpactDataSet, IImpactDataSetGetter>("Impacts") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IImpactDataSetGetter impactDataSet)
            {
                throw new InvalidOperationException($"Expected IImpactDataSetGetter but got {winningContext.Record.GetType()}");
            }

            return impactDataSet
                .ToLink<IImpactDataSetGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IImpactDataSet, IImpactDataSetGetter>(state.LinkCache)
                .ToArray();
        }
    }
}