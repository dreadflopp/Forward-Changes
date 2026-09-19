using System;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.IdleMarker
{
    public class FlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.IdleMarker.Flag>
    {
        public override string PropertyName => "Flags";

        public override Mutagen.Bethesda.Skyrim.IdleMarker.Flag GetValue(IMajorRecordGetter record)
        {
            if (record is IIdleMarkerGetter idleMarker)
            {
                return idleMarker.Flags ?? 0;
            }

            return 0;
        }

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.IdleMarker.Flag value)
        {
            if (record is IIdleMarker idleMarker)
            {
                idleMarker.Flags = value;
            }
        }

        protected override Mutagen.Bethesda.Skyrim.IdleMarker.Flag[] GetAllFlags()
        {
            return Enum.GetValues<Mutagen.Bethesda.Skyrim.IdleMarker.Flag>();
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.IdleMarker.Flag flags, Mutagen.Bethesda.Skyrim.IdleMarker.Flag flag)
        {
            return (flags & flag) == flag;
        }

        protected override Mutagen.Bethesda.Skyrim.IdleMarker.Flag SetFlag(Mutagen.Bethesda.Skyrim.IdleMarker.Flag flags, Mutagen.Bethesda.Skyrim.IdleMarker.Flag flag, bool value)
        {
            if (value)
            {
                return flags | flag;
            }

            return flags & ~flag;
        }
    }
}
