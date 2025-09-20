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
    public class LitWaterHandler : AbstractListPropertyHandler<IFormLinkGetter<IPlacedObjectGetter>>
    {
        public override string PropertyName => "LitWater";

        public override void SetValue(IMajorRecord record, List<IFormLinkGetter<IPlacedObjectGetter>>? value)
        {
            if (record is IPlacedObject placedObjectRecord)
            {
                placedObjectRecord.LitWater?.Clear();
                if (value != null && placedObjectRecord.LitWater != null)
                {
                    foreach (var waterLink in value)
                    {
                        if (waterLink == null) continue;
                        if (!waterLink.FormKey.IsNull)
                        {
                            placedObjectRecord.LitWater.Add(new FormLink<IPlacedObjectGetter>(waterLink.FormKey));
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            }
        }

        public override List<IFormLinkGetter<IPlacedObjectGetter>>? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObjectRecord)
            {
                return placedObjectRecord.LitWater?.ToList();
            }
            Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            return null;
        }

        protected override bool IsItemEqual(IFormLinkGetter<IPlacedObjectGetter>? item1, IFormLinkGetter<IPlacedObjectGetter>? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            // Compare FormKeys
            return item1.FormKey.Equals(item2.FormKey);
        }

        protected override string FormatItem(IFormLinkGetter<IPlacedObjectGetter>? item)
        {
            if (item == null) return "null";
            return $"FormLink<IPlacedObjectGetter>({item.FormKey})";
        }
    }
}