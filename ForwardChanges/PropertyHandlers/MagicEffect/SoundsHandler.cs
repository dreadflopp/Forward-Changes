using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Noggog;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.MagicEffect
{
    public class SoundsHandler : AbstractListPropertyHandler<IMagicEffectSoundGetter>
    {
        public override string PropertyName => "Sounds";
        protected override ListOrdering Ordering => ListOrdering.PreserveModOrder;

        public override List<IMagicEffectSoundGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IMagicEffectGetter magicEffect)
            {
                return magicEffect.Sounds?.ToList();
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffectGetter for {PropertyName}");
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, List<IMagicEffectSoundGetter>? value)
        {
            if (record is IMagicEffect magicEffect)
            {
                if (value == null)
                {
                    magicEffect.Sounds = null;
                    return;
                }

                // Create a new list and deep copy all sounds
                var newSounds = new ExtendedList<MagicEffectSound>();
                foreach (var sound in value)
                {
                    if (sound == null) continue;

                    var newSound = new MagicEffectSound();
                    newSound.DeepCopyIn(sound);
                    newSounds.Add(newSound);
                }

                magicEffect.Sounds = newSounds;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffect for {PropertyName}");
            }
        }

        protected override bool IsItemEqual(IMagicEffectSoundGetter? item1, IMagicEffectSoundGetter? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            // Compare sound properties
            return item1.Type == item2.Type &&
                   item1.Sound.FormKey == item2.Sound.FormKey;
        }

        protected override string FormatItem(IMagicEffectSoundGetter? item)
        {
            if (item == null) return "null";
            return $"Sound(Type: {item.Type}, Sound: {item.Sound})";
        }
    }
}
