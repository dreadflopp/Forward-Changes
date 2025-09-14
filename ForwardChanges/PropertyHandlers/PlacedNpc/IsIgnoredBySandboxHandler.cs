using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.PlacedNpc
{
    public class IsIgnoredBySandboxHandler : AbstractPropertyHandler<bool>
    {
        public override string PropertyName => "IsIgnoredBySandbox";

        public override void SetValue(IMajorRecord record, bool value)
        {
            if (record is IPlacedNpc placedNpcRecord)
            {
                placedNpcRecord.IsIgnoredBySandbox = value;
            }
        }

        public override bool GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedNpcGetter placedNpcRecord)
            {
                return placedNpcRecord.IsIgnoredBySandbox;
            }
            return false;
        }
    }
}