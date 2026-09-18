using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Ingestible;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: ObjectBounds, Description, PickUpSound, PutDownSound, EquipmentType, Addiction, AddictionChance, ConsumeSound.
    // - Generalized Effects reconciliation to the shared exact-position atomic handler.
    // - Kept specialized: Destructible, Icons, Effects collection access, Flags, MajorFlags.
    // - Rationale: direct properties are reflection-safe; xEdit gives outer Effects
    //   entries no row key, while collection access and flag handling remain record-specific.
    public class IngestibleRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "ObjectBounds", new ObjectBoundsHandler() },
            { "Name", new NameHandler() },
            { "Description", new ComplexReflectionPropertyHandler<ITranslatedStringGetter, IIngestible, IIngestibleGetter>("Description") },
            { "Model", new ModelHandler() },
            { "Destructible", new DestructibleHandler() },
            { "Icons", new IconsHandler() },
            { "PickUpSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IIngestible, IIngestibleGetter>("PickUpSound") },
            { "PutDownSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IIngestible, IIngestibleGetter>("PutDownSound") },
            { "EquipmentType", new SimpleReflectionFormLinkPropertyHandler<IEquipTypeGetter, IIngestible, IIngestibleGetter>("EquipmentType") },
            { "Weight", new WeightHandler() },
            { "Value", new ValueHandler() },
            { "Keywords", new KeywordListHandler() },
            { "Addiction", new SimpleReflectionFormLinkPropertyHandler<ISkyrimMajorRecordGetter, IIngestible, IIngestibleGetter>("Addiction") },
            { "AddictionChance", new SimpleReflectionPropertyHandler<float, IIngestible, IIngestibleGetter>("AddictionChance", 0.001f) },
            { "ConsumeSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IIngestible, IIngestibleGetter>("ConsumeSound") },
            { "Effects", new EffectHandler() },
            { "Flags", new FlagsHandler() },
            { "MajorFlags", new MajorFlagsHandler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IIngestibleGetter ingestibleRecord)
            {
                throw new InvalidOperationException($"Expected IIngestibleGetter but got {winningContext.Record.GetType()}");
            }
            var contexts = ingestibleRecord
                .ToLink<IIngestibleGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IIngestible, IIngestibleGetter>(state.LinkCache)
                .ToArray();

            return contexts;
        }

        // GetOverrideRecord and ApplyForwardedProperties are now handled by the base class
        // The base class automatically handles flag property coordination
    }
}
