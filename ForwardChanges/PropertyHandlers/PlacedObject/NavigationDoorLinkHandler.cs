using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class NavigationDoorLinkHandler : AbstractPropertyHandler<INavigationDoorLinkGetter?>
    {
        public override string PropertyName => "NavigationDoorLink";

        public override void SetValue(IMajorRecord record, INavigationDoorLinkGetter? value)
        {
            if (record is IPlacedObject placedObjectRecord)
            {
                if (value != null)
                {
                    // Create new NavigationDoorLink and copy properties
                    var newDoorLink = new NavigationDoorLink
                    {
                        NavMesh = new FormLink<INavigationMeshGetter>(value.NavMesh.FormKey),
                        TeleportMarkerTriangle = value.TeleportMarkerTriangle,
                        Unused = value.Unused
                    };
                    placedObjectRecord.NavigationDoorLink = newDoorLink;
                }
                else
                {
                    placedObjectRecord.NavigationDoorLink = null;
                }
            }
        }

        public override INavigationDoorLinkGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObjectRecord)
            {
                return placedObjectRecord.NavigationDoorLink;
            }
            Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            return null;
        }

        public override bool AreValuesEqual(INavigationDoorLinkGetter? value1, INavigationDoorLinkGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare all properties
            if (!value1.NavMesh.FormKey.Equals(value2.NavMesh.FormKey)) return false;
            if (value1.TeleportMarkerTriangle != value2.TeleportMarkerTriangle) return false;
            if (value1.Unused != value2.Unused) return false;

            return true;
        }
    }
}