using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.MagicEffect
{
    public class CounterEffectsHandler : AbstractListPropertyHandler<IFormLinkGetter<IMagicEffectGetter>>
    {
        public override string PropertyName => "CounterEffects";

        public override List<IFormLinkGetter<IMagicEffectGetter>>? GetValue(IMajorRecordGetter record)
        {
            if (record is IMagicEffectGetter magicEffect)
            {
                return magicEffect.CounterEffects?.ToList();
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffectGetter for {PropertyName}");
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, List<IFormLinkGetter<IMagicEffectGetter>>? value)
        {
            if (record is IMagicEffect magicEffect)
            {
                if (value == null)
                {
                    magicEffect.CounterEffects.Clear();
                    return;
                }

                // Clear existing counter effects and add new ones
                magicEffect.CounterEffects.Clear();
                foreach (var counterEffect in value)
                {
                    if (counterEffect == null) continue;
                    magicEffect.CounterEffects.Add(counterEffect);
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffect for {PropertyName}");
            }
        }

        protected override bool IsItemEqual(IFormLinkGetter<IMagicEffectGetter>? item1, IFormLinkGetter<IMagicEffectGetter>? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            // Compare FormKeys instead of using Equals on complex FormLink objects
            return item1.FormKey == item2.FormKey;
        }

        protected override string FormatItem(IFormLinkGetter<IMagicEffectGetter>? item)
        {
            if (item == null) return "null";
            return $"CounterEffect({item.FormKey})";
        }
    }
}
