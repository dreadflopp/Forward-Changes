using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Strings;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class MapMarkerHandler : AbstractPropertyHandler<IMapMarkerGetter?>
    {
        public override string PropertyName => "MapMarker";

        public override void SetValue(IMajorRecord record, IMapMarkerGetter? value)
        {
            if (record is IPlacedObject placedObjectRecord)
            {
                if (value != null)
                {
                    // Create new MapMarker and copy properties
                    var newMapMarker = new MapMarker
                    {
                        Flags = value.Flags,
                        Name = value.Name != null ? new TranslatedString(Language.English, value.Name.String) : null,
                        Type = value.Type
                    };
                    placedObjectRecord.MapMarker = newMapMarker;
                }
                else
                {
                    placedObjectRecord.MapMarker = null;
                }
            }
        }

        public override IMapMarkerGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObjectRecord)
            {
                return placedObjectRecord.MapMarker;
            }
            Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            return null;
        }

        public override bool AreValuesEqual(IMapMarkerGetter? value1, IMapMarkerGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare all properties
            if (value1.Flags != value2.Flags) return false;
            if (value1.Name?.String != value2.Name?.String) return false;
            if (value1.Type != value2.Type) return false;

            return true;
        }
    }
}