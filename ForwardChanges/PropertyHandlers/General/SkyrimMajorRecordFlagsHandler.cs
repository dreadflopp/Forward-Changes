using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.General
{
    public class SkyrimMajorRecordFlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.SkyrimMajorRecord.SkyrimMajorRecordFlag>
    {
        public override string PropertyName => "SkyrimMajorRecordFlags";

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.SkyrimMajorRecord.SkyrimMajorRecordFlag value)
        {
            if (record is ISkyrimMajorRecord skyrimRecord)
            {
                skyrimRecord.SkyrimMajorRecordFlags = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement ISkyrimMajorRecord for {PropertyName}");
            }
        }

        public override Mutagen.Bethesda.Skyrim.SkyrimMajorRecord.SkyrimMajorRecordFlag GetValue(IMajorRecordGetter record)
        {
            if (record is ISkyrimMajorRecordGetter skyrimRecord)
            {
                return skyrimRecord.SkyrimMajorRecordFlags;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement ISkyrimMajorRecordGetter for {PropertyName}");
            }
            return default(Mutagen.Bethesda.Skyrim.SkyrimMajorRecord.SkyrimMajorRecordFlag);
        }

        protected override Mutagen.Bethesda.Skyrim.SkyrimMajorRecord.SkyrimMajorRecordFlag[] GetAllFlags()
        {
            return Enum.GetValues<Mutagen.Bethesda.Skyrim.SkyrimMajorRecord.SkyrimMajorRecordFlag>();
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.SkyrimMajorRecord.SkyrimMajorRecordFlag flags, Mutagen.Bethesda.Skyrim.SkyrimMajorRecord.SkyrimMajorRecordFlag flag)
        {
            return flags.HasFlag(flag);
        }

        protected override Mutagen.Bethesda.Skyrim.SkyrimMajorRecord.SkyrimMajorRecordFlag SetFlag(Mutagen.Bethesda.Skyrim.SkyrimMajorRecord.SkyrimMajorRecordFlag flags, Mutagen.Bethesda.Skyrim.SkyrimMajorRecord.SkyrimMajorRecordFlag flag, bool value)
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
