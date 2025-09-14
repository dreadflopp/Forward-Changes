using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.PlacedNpc
{
    public class CountHandler : AbstractPropertyHandler<int?>
    {
        public override string PropertyName => "Count";

        public override void SetValue(IMajorRecord record, int? value)
        {
            if (record is IPlacedNpc placedNpcRecord)
            {
                placedNpcRecord.Count = value;
            }
        }

        public override int? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedNpcGetter placedNpcRecord)
            {
                return placedNpcRecord.Count;
            }
            return null;
        }
    }
}