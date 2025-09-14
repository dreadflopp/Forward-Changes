using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.ObjectEffect;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System;

namespace ForwardChanges.RecordHandlers
{
    public class ObjectEffectRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Name", new NameHandler() },
            { "ObjectBounds", new ObjectBoundsHandler() },
            { "EnchantmentCost", new EnchantmentCostHandler() },
            { "CastType", new CastTypeHandler() },
            { "EnchantmentAmount", new EnchantmentAmountHandler() },
            { "TargetType", new TargetTypeHandler() },
            { "EnchantType", new EnchantTypeHandler() },
            { "ChargeTime", new ChargeTimeHandler() },
            { "BaseEnchantment", new BaseEnchantmentHandler() },
            { "WornRestrictions", new WornRestrictionsHandler() },
            { "Effects", new EffectsHandler() },
            { "Flags", new FlagsHandler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IObjectEffectGetter objectEffectRecord)
            {
                throw new InvalidOperationException($"Expected IObjectEffectGetter but got {winningContext.Record.GetType()}");
            }
            var contexts = objectEffectRecord
                .ToLink<IObjectEffectGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IObjectEffect, IObjectEffectGetter>(state.LinkCache)
                .ToArray();

            return contexts;
        }

        public override IMajorRecord GetOverrideRecord(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
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
                        handler.SetValue(record, value);
                    }
                    catch (Exception ex)
                    {
                        // Property doesn't exist on this object effect type - just continue
                        Console.WriteLine($"Warning: Property {propertyName} not available on object effect {record.FormKey}: {ex.Message}");
                        Console.WriteLine($"Exception type: {ex.GetType().Name}");
                        Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    }
                }
            }
        }
    }
}