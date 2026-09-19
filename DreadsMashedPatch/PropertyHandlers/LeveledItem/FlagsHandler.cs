using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.LeveledItem
{
    public class FlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.LeveledItem.Flag>
    {
        public override string PropertyName => "Flags";

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.LeveledItem.Flag value)
        {
            if (record is ILeveledItem leveledItem)
            {
                leveledItem.Flags = value;
            }
            else
            {
                System.Console.WriteLine($"Error: Record does not implement ILeveledItem for {PropertyName}");
            }
        }

        public override Mutagen.Bethesda.Skyrim.LeveledItem.Flag GetValue(IMajorRecordGetter record)
        {
            if (record is ILeveledItemGetter leveledItem)
            {
                return leveledItem.Flags;
            }
            else
            {
                System.Console.WriteLine($"Error: Record does not implement ILeveledItemGetter for {PropertyName}");
            }
            return default(Mutagen.Bethesda.Skyrim.LeveledItem.Flag);
        }

        protected override Mutagen.Bethesda.Skyrim.LeveledItem.Flag[] GetAllFlags()
        {
            return Enum.GetValues<Mutagen.Bethesda.Skyrim.LeveledItem.Flag>();
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.LeveledItem.Flag flags, Mutagen.Bethesda.Skyrim.LeveledItem.Flag flag)
        {
            return (flags & flag) == flag;
        }

        protected override Mutagen.Bethesda.Skyrim.LeveledItem.Flag SetFlag(Mutagen.Bethesda.Skyrim.LeveledItem.Flag flags, Mutagen.Bethesda.Skyrim.LeveledItem.Flag flag, bool value)
        {
            if (value)
            {
                return flags | flag;
            }
            else
            {
                return flags & ~flag;
            }
        }
    }
}
