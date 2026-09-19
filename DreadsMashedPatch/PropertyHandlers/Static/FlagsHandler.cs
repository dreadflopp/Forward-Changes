using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Static
{
    public class FlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Static.Flag>
    {
        public override string PropertyName => "Flags";

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.Static.Flag value)
        {
            if (record is IStatic staticRecord)
            {
                staticRecord.Flags = value;
            }
            else
            {
                System.Console.WriteLine($"Error: Record does not implement IStatic for {PropertyName}");
            }
        }

        public override Mutagen.Bethesda.Skyrim.Static.Flag GetValue(IMajorRecordGetter record)
        {
            if (record is IStaticGetter staticRecord)
            {
                return staticRecord.Flags;
            }
            else
            {
                System.Console.WriteLine($"Error: Record does not implement IStaticGetter for {PropertyName}");
            }
            return default(Mutagen.Bethesda.Skyrim.Static.Flag);
        }

        protected override Mutagen.Bethesda.Skyrim.Static.Flag[] GetAllFlags()
        {
            return Enum.GetValues<Mutagen.Bethesda.Skyrim.Static.Flag>();
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.Static.Flag flags, Mutagen.Bethesda.Skyrim.Static.Flag flag)
        {
            return (flags & flag) == flag;
        }

        protected override Mutagen.Bethesda.Skyrim.Static.Flag SetFlag(Mutagen.Bethesda.Skyrim.Static.Flag flags, Mutagen.Bethesda.Skyrim.Static.Flag flag, bool value)
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
