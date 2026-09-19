using System;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Light
{
    public class FlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Light.Flag>
    {
        public override string PropertyName => "Flags";

        public override Mutagen.Bethesda.Skyrim.Light.Flag GetValue(IMajorRecordGetter record)
        {
            if (record is ILightGetter lightRecord)
            {
                return lightRecord.Flags;
            }
            return 0;
        }

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.Light.Flag value)
        {
            if (record is Mutagen.Bethesda.Skyrim.Light lightRecord)
            {
                lightRecord.Flags = value;
            }
        }

        protected override Mutagen.Bethesda.Skyrim.Light.Flag[] GetAllFlags()
        {
            return Enum.GetValues<Mutagen.Bethesda.Skyrim.Light.Flag>();
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.Light.Flag flags, Mutagen.Bethesda.Skyrim.Light.Flag flag)
        {
            return (flags & flag) == flag;
        }

        protected override Mutagen.Bethesda.Skyrim.Light.Flag SetFlag(Mutagen.Bethesda.Skyrim.Light.Flag flags, Mutagen.Bethesda.Skyrim.Light.Flag flag, bool value)
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

