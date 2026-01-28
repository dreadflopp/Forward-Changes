using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Noggog;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.LeveledItem
{
    public class ChanceNoneHandler : AbstractPropertyHandler<Percent>
    {
        public override string PropertyName => "ChanceNone";

        public override void SetValue(IMajorRecord record, Percent value)
        {
            var leveledItem = TryCastRecord<ILeveledItem>(record, PropertyName);
            if (leveledItem != null)
            {
                leveledItem.ChanceNone = value;
            }
        }

        public override Percent GetValue(IMajorRecordGetter record)
        {
            var leveledItem = TryCastRecord<ILeveledItemGetter>(record, PropertyName);
            if (leveledItem != null)
            {
                return leveledItem.ChanceNone;
            }
            return Percent.Zero;
        }

        public override bool AreValuesEqual(Percent value1, Percent value2)
        {
            return value1.Equals(value2);
        }
    }
}
