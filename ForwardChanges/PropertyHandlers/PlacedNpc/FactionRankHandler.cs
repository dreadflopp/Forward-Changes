using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.PlacedNpc
{
    public class FactionRankHandler : AbstractPropertyHandler<int?>
    {
        public override string PropertyName => "FactionRank";

        public override void SetValue(IMajorRecord record, int? value)
        {
            if (record is IPlacedNpc placedNpcRecord)
            {
                placedNpcRecord.FactionRank = value;
            }
        }

        public override int? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedNpcGetter placedNpcRecord)
            {
                return placedNpcRecord.FactionRank;
            }
            return null;
        }
    }
}