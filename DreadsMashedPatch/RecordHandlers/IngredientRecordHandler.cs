using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using DreadsMashedPatch.PropertyHandlers.Ingredient;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.RecordHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using System;

namespace DreadsMashedPatch.RecordHandlers
{
    // Effects migration note: replaced whole-list property handling with the shared
    // exact-position atomic handler; ingredient collection access stays specialized
    // because xEdit gives the outer Effects entries no stable row key.
    public class IngredientRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Name", new NameHandler() },
            { "VirtualMachineAdapter", new VirtualMachineAdapterHandler() },
            { "ObjectBounds", new ObjectBoundsHandler() },
            { "Model", new ModelHandler() },
            { "Icons", new IconsHandler() },
            { "Destructible", new DestructibleHandler() },
            { "EquipType", new SimpleReflectionFormLinkPropertyHandler<IEquipTypeGetter, IIngredient, IIngredientGetter>("EquipType") },
            { "PickUpSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IIngredient, IIngredientGetter>("PickUpSound") },
            { "PutDownSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IIngredient, IIngredientGetter>("PutDownSound") },
            { "Value", new ValueHandler() },
            { "Weight", new WeightHandler() },
            { "IngredientValue", new SimpleReflectionPropertyHandler<int, IIngredient, IIngredientGetter>("IngredientValue") },
            { "Flags", new FlagsHandler() },
            { "Effects", new EffectHandler() },
            { "Keywords", new KeywordListHandler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IIngredientGetter ingredientRecord)
            {
                throw new InvalidOperationException($"Expected IIngredientGetter but got {winningContext.Record.GetType()}");
            }
            var contexts = ingredientRecord
                .ToLink<IIngredientGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IIngredient, IIngredientGetter>(state.LinkCache)
                .ToArray();

            return contexts;
        }

        // GetOverrideRecord and ApplyForwardedProperties are now handled by the base class
        // The base class automatically handles flag property coordination
    }
}
