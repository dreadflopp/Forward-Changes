using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using Noggog;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class ReflectionsHandler : AbstractListPropertyHandler<IWaterReflectionGetter>
    {
        public override string PropertyName => "Reflections";

        public override void SetValue(IMajorRecord record, List<IWaterReflectionGetter>? value)
        {
            if (record is IPlacedObject placedObjectRecord)
            {
                placedObjectRecord.Reflections?.Clear();
                if (value != null && placedObjectRecord.Reflections != null)
                {
                    foreach (var reflection in value)
                    {
                        if (reflection == null) continue;
                        var newReflection = new WaterReflection
                        {
                            Versioning = reflection.Versioning,
                            Water = new FormLink<IPlacedObjectGetter>(reflection.Water.FormKey),
                            Type = reflection.Type
                        };
                        placedObjectRecord.Reflections.Add(newReflection);
                    }
                }
            }
            else
            {
                Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            }
        }

        public override List<IWaterReflectionGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObjectRecord)
            {
                return placedObjectRecord.Reflections?.ToList();
            }
            Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            return null;
        }

        protected override bool IsItemEqual(IWaterReflectionGetter? item1, IWaterReflectionGetter? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            // Compare properties
            if (item1.Versioning != item2.Versioning) return false;
            if (!item1.Water.FormKey.Equals(item2.Water.FormKey)) return false;
            if (item1.Type != item2.Type) return false;

            return true;
        }

        protected override string FormatItem(IWaterReflectionGetter? item)
        {
            if (item == null) return "null";
            return $"WaterReflection(Water: {item.Water.FormKey}, Type: {item.Type})";
        }
    }
}