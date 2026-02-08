using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Ingestible;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System;

namespace ForwardChanges.RecordHandlers
{
    public class IngestibleRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Name", new NameHandler() },
            { "Description", new DescriptionHandler() },
            { "Model", new ModelHandler() },
            { "Destructible", new DestructibleHandler() },
            { "Icons", new IconsHandler() },
            { "PickUpSound", new PickUpSoundHandler() },
            { "PutDownSound", new PutDownSoundHandler() },
            { "EquipmentType", new EquipmentTypeHandler() },
            { "Weight", new WeightHandler() },
            { "Value", new ValueHandler() },
            { "Keywords", new KeywordListHandler() },
            { "Addiction", new AddictionHandler() },
            { "AddictionChance", new AddictionChanceHandler() },
            { "ConsumeSound", new ConsumeSoundHandler() },
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