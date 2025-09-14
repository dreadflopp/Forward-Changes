using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.PlacedNpc
{
    public class LevelModifierHandler : AbstractPropertyHandler<Level?>
    {
        public override string PropertyName => "LevelModifier";

        public override void SetValue(IMajorRecord record, Level? value)
        {
            if (record is IPlacedNpc placedNpcRecord)
            {
                placedNpcRecord.LevelModifier = value;
            }
        }

        public override Level? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedNpcGetter placedNpcRecord)
            {
                return placedNpcRecord.LevelModifier;
            }
            return null;
        }
    }
}