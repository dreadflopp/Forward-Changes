using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System;
using ForwardChanges.PropertyHandlers.ListPropertyHandlers;

namespace ForwardChanges.RecordHandlers
{
    public class IngredientRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDPropertyHandler() },
            { "Name", new NamePropertyHandler() },
            { "Model", new ModelPropertyHandler() },
            { "Value", new ValuePropertyHandler() },
            { "Weight", new WeightPropertyHandler() },
            { "IngredientValue", new IngredientValuePropertyHandler() },
            { "Effects", new IngredientEffectPropertyHandler() },
            { "Keywords", new KeywordListPropertyHandler() }
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

        public override IMajorRecord GetOverrideRecord(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            // Use the context's GetOrAddAsOverride method like Fusion does
            return winningContext.GetOrAddAsOverride(state.PatchMod);
        }

        public override void ApplyForwardedProperties(IMajorRecord record, Dictionary<string, object?> propertiesToForward)
        {
            foreach (var (propertyName, value) in propertiesToForward)
            {
                if (PropertyHandlers.TryGetValue(propertyName, out var handler))
                {
                    try
                    {
                        Console.WriteLine($"[{propertyName}] Applying value: {handler.FormatValue(value)}, Type: {value?.GetType()}");
                        if (value != null)
                        {
                            handler.SetValue(record, value);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Property doesn't exist on this ingredient type - just continue
                        Console.WriteLine($"Warning: Property {propertyName} not available on ingredient {record.FormKey}: {ex.Message}");
                    }
                }
            }
        }
    }
}