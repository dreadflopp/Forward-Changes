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
    public class PortalsHandler : AbstractListPropertyHandler<IPortalGetter>
    {
        public override string PropertyName => "Portals";

        public override void SetValue(IMajorRecord record, List<IPortalGetter>? value)
        {
            if (record is IPlacedObject placedObjectRecord)
            {
                placedObjectRecord.Portals?.Clear();
                if (value != null)
                {
                    foreach (var portal in value)
                    {
                        var newPortal = new Portal
                        {
                            Origin = new FormLink<IPlacedObjectGetter>(portal.Origin.FormKey),
                            Destination = new FormLink<IPlacedObjectGetter>(portal.Destination.FormKey)
                        };
                        placedObjectRecord.Portals?.Add(newPortal);
                    }
                }
            }
        }

        public override List<IPortalGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObjectRecord)
            {
                return placedObjectRecord.Portals?.ToList();
            }
            Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            return null;
        }

        protected override bool IsItemEqual(IPortalGetter? item1, IPortalGetter? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            // Compare Origin and Destination FormKeys
            if (!item1.Origin.FormKey.Equals(item2.Origin.FormKey)) return false;
            if (!item1.Destination.FormKey.Equals(item2.Destination.FormKey)) return false;

            return true;
        }

        protected override string FormatItem(IPortalGetter? item)
        {
            if (item == null) return "null";
            return $"Portal(Origin: {item.Origin.FormKey}, Destination: {item.Destination.FormKey})";
        }
    }
}