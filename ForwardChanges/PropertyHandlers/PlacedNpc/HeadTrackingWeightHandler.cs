using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.PlacedNpc
{
    public class HeadTrackingWeightHandler : AbstractPropertyHandler<float?>
    {
        public override string PropertyName => "HeadTrackingWeight";

        public override void SetValue(IMajorRecord record, float? value)
        {
            if (record is IPlacedNpc placedNpcRecord)
            {
                placedNpcRecord.HeadTrackingWeight = value;
            }
        }

        public override float? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedNpcGetter placedNpcRecord)
            {
                return placedNpcRecord.HeadTrackingWeight;
            }
            return null;
        }
    }
}