using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Location
{
    public class WorldLocationRadiusHandler : AbstractPropertyHandler<float?>
    {
        public override string PropertyName => "WorldLocationRadius";

        public override float? GetValue(IMajorRecordGetter record)
        {
            var locationRecord = TryCastRecord<ILocationGetter>(record, PropertyName);
            if (locationRecord != null)
            {
                return locationRecord.WorldLocationRadius;
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, float? value)
        {
            var locationRecord = TryCastRecord<ILocation>(record, PropertyName);
            if (locationRecord != null)
            {
                locationRecord.WorldLocationRadius = value;
            }
        }
    }
}

