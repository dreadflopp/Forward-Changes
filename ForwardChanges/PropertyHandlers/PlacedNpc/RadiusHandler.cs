using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.PlacedNpc
{
    public class RadiusHandler : AbstractPropertyHandler<float?>
    {
        public override string PropertyName => "Radius";

        public override void SetValue(IMajorRecord record, float? value)
        {
            if (record is IPlacedNpc placedNpcRecord)
            {
                placedNpcRecord.Radius = value;
            }
        }

        public override float? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedNpcGetter placedNpcRecord)
            {
                return placedNpcRecord.Radius;
            }
            return null;
        }
    }
}