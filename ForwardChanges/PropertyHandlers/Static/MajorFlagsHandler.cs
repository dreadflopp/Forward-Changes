using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Static
{
    public class MajorFlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Static.MajorFlag>
    {
        public override string PropertyName => "MajorFlags";

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.Static.MajorFlag value)
        {
            if (record is IStatic staticRecord)
            {
                staticRecord.MajorFlags = value;
            }
            else
            {
                System.Console.WriteLine($"Error: Record does not implement IStatic for {PropertyName}");
            }
        }

        public override Mutagen.Bethesda.Skyrim.Static.MajorFlag GetValue(IMajorRecordGetter record)
        {
            if (record is IStaticGetter staticRecord)
            {
                return staticRecord.MajorFlags;
            }
            else
            {
                System.Console.WriteLine($"Error: Record does not implement IStaticGetter for {PropertyName}");
            }
            return default(Mutagen.Bethesda.Skyrim.Static.MajorFlag);
        }

        protected override Mutagen.Bethesda.Skyrim.Static.MajorFlag[] GetAllFlags()
        {
            return Enum.GetValues<Mutagen.Bethesda.Skyrim.Static.MajorFlag>();
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.Static.MajorFlag flags, Mutagen.Bethesda.Skyrim.Static.MajorFlag flag)
        {
            return (flags & flag) == flag;
        }

        protected override Mutagen.Bethesda.Skyrim.Static.MajorFlag SetFlag(Mutagen.Bethesda.Skyrim.Static.MajorFlag flags, Mutagen.Bethesda.Skyrim.Static.MajorFlag flag, bool value)
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
