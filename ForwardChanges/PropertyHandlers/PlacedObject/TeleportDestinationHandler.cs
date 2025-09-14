using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using Noggog;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class TeleportDestinationHandler : AbstractPropertyHandler<ITeleportDestinationGetter?>
    {
        public override string PropertyName => "TeleportDestination";

        public override void SetValue(IMajorRecord record, ITeleportDestinationGetter? value)
        {
            if (record is IPlacedObject placedObjectRecord)
            {
                if (value != null)
                {
                    // Create new TeleportDestination and copy properties
                    var newTeleportDest = new TeleportDestination
                    {
                        Door = new FormLink<IPlacedObjectGetter>(value.Door.FormKey),
                        Position = value.Position,
                        Rotation = value.Rotation,
                        Flags = value.Flags
                    };
                    placedObjectRecord.TeleportDestination = newTeleportDest;
                }
                else
                {
                    placedObjectRecord.TeleportDestination = null;
                }
            }
        }

        public override ITeleportDestinationGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObjectRecord)
            {
                return placedObjectRecord.TeleportDestination;
            }
            Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            return null;
        }

        public override bool AreValuesEqual(ITeleportDestinationGetter? value1, ITeleportDestinationGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare all properties
            if (!value1.Door.FormKey.Equals(value2.Door.FormKey)) return false;
            if (!value1.Position.Equals(value2.Position)) return false;
            if (!value1.Rotation.Equals(value2.Rotation)) return false;
            if (value1.Flags != value2.Flags) return false;

            return true;
        }
    }
}