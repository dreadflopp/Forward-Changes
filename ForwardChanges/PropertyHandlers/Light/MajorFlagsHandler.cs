using System;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Light
{
    public class MajorFlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Light.MajorFlag>
    {
        public override string PropertyName => "MajorFlags";

        public override Mutagen.Bethesda.Skyrim.Light.MajorFlag GetValue(IMajorRecordGetter record)
        {
            if (record is ILightGetter lightRecord)
            {
                return lightRecord.MajorFlags;
            }
            return 0;
        }

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.Light.MajorFlag value)
        {
            if (record is Mutagen.Bethesda.Skyrim.Light lightRecord)
            {
                lightRecord.MajorFlags = value;
            }
        }

        protected override Mutagen.Bethesda.Skyrim.Light.MajorFlag[] GetAllFlags()
        {
            return Enum.GetValues<Mutagen.Bethesda.Skyrim.Light.MajorFlag>();
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.Light.MajorFlag flags, Mutagen.Bethesda.Skyrim.Light.MajorFlag flag)
        {
            return (flags & flag) == flag;
        }

        protected override Mutagen.Bethesda.Skyrim.Light.MajorFlag SetFlag(Mutagen.Bethesda.Skyrim.Light.MajorFlag flags, Mutagen.Bethesda.Skyrim.Light.MajorFlag flag, bool value)
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

