using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class LocationRefTypesHandler : AbstractListPropertyHandler<IFormLinkGetter<ILocationReferenceTypeGetter>>
    {
        public override string PropertyName => "LocationRefTypes";

        public override void SetValue(IMajorRecord record, List<IFormLinkGetter<ILocationReferenceTypeGetter>>? value)
        {
            if (record is IPlacedObject placedObjectRecord)
            {
                placedObjectRecord.LocationRefTypes?.Clear();
                if (value != null)
                {
                    foreach (var refTypeLink in value)
                    {
                        if (!refTypeLink.FormKey.IsNull)
                        {
                            placedObjectRecord.LocationRefTypes?.Add(new FormLink<ILocationReferenceTypeGetter>(refTypeLink.FormKey));
                        }
                    }
                }
            }
        }

        public override List<IFormLinkGetter<ILocationReferenceTypeGetter>>? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObjectRecord)
            {
                return placedObjectRecord.LocationRefTypes?.ToList();
            }
            Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            return null;
        }

        protected override bool IsItemEqual(IFormLinkGetter<ILocationReferenceTypeGetter>? item1, IFormLinkGetter<ILocationReferenceTypeGetter>? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            // Compare FormKeys
            return item1.FormKey.Equals(item2.FormKey);
        }

        protected override string FormatItem(IFormLinkGetter<ILocationReferenceTypeGetter>? item)
        {
            if (item == null) return "null";
            return $"FormLink<ILocationReferenceTypeGetter>({item.FormKey})";
        }
    }
}