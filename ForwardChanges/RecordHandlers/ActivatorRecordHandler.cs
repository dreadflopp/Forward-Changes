using System.Drawing;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Activator;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.RecordHandlers
{
    public class ActivatorRecordHandler : AbstractRecordHandler
    {
        private readonly Dictionary<string, IPropertyHandler> _propertyHandlers;

        public ActivatorRecordHandler()
        {
            // Initialize property handlers for Activator records.
            // Uses reflection-based handlers from General where possible (same approach as PlacedObjectRecordHandler).
            _propertyHandlers = new Dictionary<string, IPropertyHandler>
            {
                { "EditorID", new EditorIDHandler() },
                { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
                { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
                { "VirtualMachineAdapter", new SimpleReflectionVirtualMachineAdapterHandler<IActivator, IActivatorGetter>() },
                { "ObjectBounds", new ObjectBoundsHandler() },
                { "Name", new NameHandler() },
                { "Model", new ModelHandler() },
                { "Destructible", new DestructibleHandler() },
                { "Keywords", new KeywordListHandler() },
                { "MarkerColor", new SimpleReflectionPropertyHandler<Color?, IActivator, IActivatorGetter>("MarkerColor") },
                { "LoopingSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IActivator, IActivatorGetter>("LoopingSound") },
                { "ActivationSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IActivator, IActivatorGetter>("ActivationSound") },
                { "WaterType", new SimpleReflectionFormLinkPropertyHandler<IWaterGetter, IActivator, IActivatorGetter>("WaterType") },
                { "ActivateTextOverride", new ComplexReflectionPropertyHandler<ITranslatedStringGetter, IActivator, IActivatorGetter>("ActivateTextOverride") },
                { "Flags", new SimpleReflectionFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Activator.Flag, IActivator, IActivatorGetter>("Flags") },
                { "MajorFlags", new SimpleReflectionFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Activator.MajorFlag, IActivator, IActivatorGetter>("MajorFlags") },
                { "InteractionKeyword", new SimpleReflectionFormLinkPropertyHandler<IKeywordGetter, IActivator, IActivatorGetter>("InteractionKeyword") }
            };
        }

        public override Dictionary<string, IPropertyHandler> PropertyHandlers => _propertyHandlers;

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IActivatorGetter activatorRecord)
            {
                throw new InvalidOperationException($"Expected IActivatorGetter but got {winningContext.Record.GetType()}");
            }
            return activatorRecord
                .ToLink<IActivatorGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IActivator, IActivatorGetter>(state.LinkCache)
                .ToArray();
        }

        // GetOverrideRecord and ApplyForwardedProperties are now handled by the base class
        // The base class automatically handles flag property coordination
    }
}
