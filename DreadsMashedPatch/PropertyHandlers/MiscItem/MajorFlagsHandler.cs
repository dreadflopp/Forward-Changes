using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.MiscItem
{
    public class MajorFlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.MiscItem.MajorFlag>
    {
        public override string PropertyName => "MajorFlags";

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.MiscItem.MajorFlag value)
        {
            if (record is IMiscItem miscItem)
            {
                miscItem.MajorFlags = value;
            }
            else
            {
                System.Console.WriteLine($"Error: Record does not implement IMiscItem for {PropertyName}");
            }
        }

        public override Mutagen.Bethesda.Skyrim.MiscItem.MajorFlag GetValue(IMajorRecordGetter record)
        {
            if (record is IMiscItemGetter miscItem)
            {
                return miscItem.MajorFlags;
            }
            else
            {
                System.Console.WriteLine($"Error: Record does not implement IMiscItemGetter for {PropertyName}");
            }
            return default(Mutagen.Bethesda.Skyrim.MiscItem.MajorFlag);
        }

        protected override Mutagen.Bethesda.Skyrim.MiscItem.MajorFlag[] GetAllFlags()
        {
            return Enum.GetValues<Mutagen.Bethesda.Skyrim.MiscItem.MajorFlag>();
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.MiscItem.MajorFlag flags, Mutagen.Bethesda.Skyrim.MiscItem.MajorFlag flag)
        {
            return (flags & flag) == flag;
        }

        protected override Mutagen.Bethesda.Skyrim.MiscItem.MajorFlag SetFlag(Mutagen.Bethesda.Skyrim.MiscItem.MajorFlag flags, Mutagen.Bethesda.Skyrim.MiscItem.MajorFlag flag, bool value)
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
