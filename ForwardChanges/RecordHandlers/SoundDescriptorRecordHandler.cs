using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using Noggog;
using ForwardChanges.PropertyHandlers.SoundDescriptor;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System;

namespace ForwardChanges.RecordHandlers
{
    public class SoundDescriptorRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Type", new SimpleReflectionPropertyHandler<SoundDescriptor.DescriptorType?, ISoundDescriptor, ISoundDescriptorGetter>("Type") },
            { "Category", new SimpleReflectionFormLinkPropertyHandler<ISoundCategoryGetter, ISoundDescriptor, ISoundDescriptorGetter>("Category") },
            { "AlternateSoundFor", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, ISoundDescriptor, ISoundDescriptorGetter>("AlternateSoundFor") },
            { "SoundFiles", new SoundFilesHandler() },
            { "OutputModel", new SimpleReflectionFormLinkPropertyHandler<ISoundOutputModelGetter, ISoundDescriptor, ISoundDescriptorGetter>("OutputModel") },
            { "String", new SimpleReflectionPropertyHandler<string?, ISoundDescriptor, ISoundDescriptorGetter>("String") },
            { "Conditions", new ConditionsHandler() },
            { "LoopAndRumble", new ComplexReflectionPropertyHandler<ISoundLoopAndRumbleGetter, ISoundDescriptor, ISoundDescriptorGetter>("LoopAndRumble") },
            { "PercentFrequencyShift", new SimpleReflectionPropertyHandler<Percent, ISoundDescriptor, ISoundDescriptorGetter>("PercentFrequencyShift") },
            { "PercentFrequencyVariance", new SimpleReflectionPropertyHandler<Percent, ISoundDescriptor, ISoundDescriptorGetter>("PercentFrequencyVariance") },
            { "Priority", new SimpleReflectionPropertyHandler<sbyte, ISoundDescriptor, ISoundDescriptorGetter>("Priority") },
            { "Variance", new SimpleReflectionPropertyHandler<sbyte, ISoundDescriptor, ISoundDescriptorGetter>("Variance") },
            { "StaticAttenuation", new SimpleReflectionPropertyHandler<float, ISoundDescriptor, ISoundDescriptorGetter>("StaticAttenuation") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not ISoundDescriptorGetter soundDescriptorRecord)
            {
                throw new InvalidOperationException($"Expected ISoundDescriptorGetter but got {winningContext.Record.GetType()}");
            }
            var contexts = soundDescriptorRecord
                .ToLink<ISoundDescriptorGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, ISoundDescriptor, ISoundDescriptorGetter>(state.LinkCache)
                .ToArray();

            return contexts;
        }

        // GetOverrideRecord and ApplyForwardedProperties are now handled by the base class
        // The base class automatically handles flag property coordination
    }
}