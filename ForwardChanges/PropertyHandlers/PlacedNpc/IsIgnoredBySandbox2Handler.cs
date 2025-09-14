using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.PlacedNpc
{
    public class IsIgnoredBySandbox2Handler : AbstractPropertyHandler<bool>
    {
        public override string PropertyName => "IsIgnoredBySandbox2";

        public override void SetValue(IMajorRecord record, bool value)
        {
            if (record is IPlacedNpc placedNpcRecord)
            {
                placedNpcRecord.IsIgnoredBySandbox2 = value;
            }
        }

        public override bool GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedNpcGetter placedNpcRecord)
            {
                return placedNpcRecord.IsIgnoredBySandbox2;
            }
            return false;
        }
    }
}