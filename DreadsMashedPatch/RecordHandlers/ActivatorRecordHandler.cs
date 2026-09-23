using System.Drawing;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Cache;
using DreadsMashedPatch.RecordHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.Activator;
using DreadsMashedPatch.PropertyHandlers.Interfaces;

namespace DreadsMashedPatch.RecordHandlers
{
    // Migration note:
    // - Generalized: scalar, translated-text, form-link, model, and VMAD fields use shared semantic handlers.
    // - Kept specialized: destructible data remains atomic; record and major flags retain approved flag handlers.
    // - Rationale: aggregate copying preserves nested binary/model state while independent fields remain mergeable.
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
                { "Name", new NameHandler() },
            { "ModelAndBounds", new ModelBoundsHandler() },
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
    }
}
