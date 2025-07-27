using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System.Drawing;

namespace ForwardChanges.PropertyHandlers.Location
{
    public class ColorHandler : AbstractPropertyHandler<Color?>
    {
        public override string PropertyName => "Color";

        public override Color? GetValue(IMajorRecordGetter record)
        {
            if (record is ILocationGetter locationRecord)
            {
                return locationRecord.Color;
            }

            Console.WriteLine($"Error: Record does not implement ILocationGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, Color? value)
        {
            if (record is ILocation locationRecord)
            {
                locationRecord.Color = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement ILocation for {PropertyName}");
            }
        }
    }
}

