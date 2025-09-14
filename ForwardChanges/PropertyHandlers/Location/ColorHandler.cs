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
            var locationRecord = TryCastRecord<ILocationGetter>(record, PropertyName);
            if (locationRecord != null)
            {
                return locationRecord.Color;
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, Color? value)
        {
            var locationRecord = TryCastRecord<ILocation>(record, PropertyName);
            if (locationRecord != null)
            {
                locationRecord.Color = value;
            }
        }
    }
}

