using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers;
using ForwardChanges.PropertyHandlers.ListPropertyHandlers;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System;

namespace ForwardChanges.RecordHandlers
{
    public class ArmorAddonRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDPropertyHandler() },
            { "WeightSliderEnabled", new ArmorAddonWeightSliderEnabledPropertyHandler() },
            { "WorldModel", new ArmorAddonWorldModelPropertyHandler() },
            { "FirstPersonModel", new ArmorAddonFirstPersonModelPropertyHandler() },
            { "AdditionalRaces", new ArmorAddonAdditionalRacesListPropertyHandler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IArmorAddonGetter armorAddonRecord)
            {
                throw new InvalidOperationException($"Expected IArmorAddonGetter but got {winningContext.Record.GetType()}");
            }
            var contexts = armorAddonRecord
                .ToLink<IArmorAddonGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IArmorAddon, IArmorAddonGetter>(state.LinkCache)
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
                        // Property doesn't exist on this armor addon type - just continue
                        Console.WriteLine($"Warning: Property {propertyName} not available on armor addon {record.FormKey}: {ex.Message}");
                    }
                }
            }
        }
    }
}