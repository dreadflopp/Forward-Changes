using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.MagicEffect
{
    public class ArchetypeHandler : AbstractPropertyHandler<IAMagicEffectArchetypeGetter>
    {
        public override string PropertyName => "Archetype";

        public override void SetValue(IMajorRecord record, IAMagicEffectArchetypeGetter? value)
        {
            if (record is IMagicEffect magicEffect)
            {
                if (value != null)
                {
                    // Create a deep copy of the archetype
                    var newArchetype = new MagicEffectArchetype();
                    newArchetype.DeepCopyIn(value);
                    magicEffect.Archetype = newArchetype;
                }
                else
                {
                    magicEffect.Archetype = new MagicEffectArchetype();
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffect for {PropertyName}");
            }
        }

        public override IAMagicEffectArchetypeGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is IMagicEffectGetter magicEffect)
            {
                return magicEffect.Archetype;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffectGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(IAMagicEffectArchetypeGetter? value1, IAMagicEffectArchetypeGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare all properties of the archetype
            return value1.Type == value2.Type &&
                   value1.ActorValue == value2.ActorValue &&
                   value1.AssociationKey.FormKey == value2.AssociationKey.FormKey;
        }

        public override string FormatValue(object? value)
        {
            if (value is IAMagicEffectArchetypeGetter archetype)
            {
                return $"Archetype(Type: {archetype.Type}, ActorValue: {archetype.ActorValue}, Association: {archetype.AssociationKey})";
            }
            return value?.ToString() ?? "null";
        }
    }
}
