using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using Noggog;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedNpc
{
    public class LocationRefTypesHandler : AbstractListPropertyHandler<IFormLinkGetter<ILocationReferenceTypeGetter>>
    {
        public override string PropertyName => "LocationRefTypes";

        public override void SetValue(IMajorRecord record, List<IFormLinkGetter<ILocationReferenceTypeGetter>>? value)
        {
            if (record is IPlacedNpc placedNpcRecord)
            {
                if (value == null)
                {
                    placedNpcRecord.LocationRefTypes = null;
                    return;
                }

                // Create a new list and copy all location reference types
                var newLocationRefTypes = new ExtendedList<IFormLinkGetter<ILocationReferenceTypeGetter>>();
                foreach (var locationRefType in value)
                {
                    if (locationRefType == null) continue;
                    if (!locationRefType.FormKey.IsNull)
                    {
                        newLocationRefTypes.Add(new FormLink<ILocationReferenceTypeGetter>(locationRefType.FormKey));
                    }
                }

                placedNpcRecord.LocationRefTypes = newLocationRefTypes;
            }
            else
            {
                Console.WriteLine($"Error: Record is not a PlacedNpc for {PropertyName}");
            }
        }

        public override List<IFormLinkGetter<ILocationReferenceTypeGetter>>? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedNpcGetter placedNpcRecord)
            {
                return placedNpcRecord.LocationRefTypes?.ToList();
            }
            return null;
        }

        protected override bool IsItemEqual(IFormLinkGetter<ILocationReferenceTypeGetter>? item1, IFormLinkGetter<ILocationReferenceTypeGetter>? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            return item1.FormKey.Equals(item2.FormKey);
        }

        protected override string FormatItem(IFormLinkGetter<ILocationReferenceTypeGetter>? item)
        {
            if (item == null) return "null";
            return item.FormKey.ToString();
        }
    }
}