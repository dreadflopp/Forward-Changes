using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Key;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: KEYM-specific VM and major flags via reflection handlers.
    // - Kept specialized: shared item handlers for bounds/model/icons/destructible/sounds/keywords/value/weight.
    // - Rationale: mirrors existing MiscItem pattern while honoring KEYM interface surface.
    public class KeyRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Name", new NameHandler() },
            { "VirtualMachineAdapter", new SimpleReflectionVirtualMachineAdapterHandler<IKey, IKeyGetter>() },
            { "ObjectBounds", new ObjectBoundsHandler() },
            { "Model", new ModelHandler() },
            { "Icons", new IconsHandler() },
            { "Destructible", new DestructibleHandler() },
            { "PickUpSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IKey, IKeyGetter>("PickUpSound") },
            { "PutDownSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IKey, IKeyGetter>("PutDownSound") },
            { "Keywords", new KeywordListHandler() },
            { "Value", new ValueHandler() },
            { "Weight", new WeightHandler() },
            { "MajorFlags", new SimpleReflectionFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Key.MajorFlag, IKey, IKeyGetter>("MajorFlags") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IKeyGetter keyRecord)
            {
                throw new InvalidOperationException($"Expected IKeyGetter but got {winningContext.Record.GetType()}");
            }

            return keyRecord
                .ToLink<IKeyGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IKey, IKeyGetter>(state.LinkCache)
                .ToArray();
        }
    }
}
