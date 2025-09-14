using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Container
{
    public class FlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Container.Flag>
    {
        public override string PropertyName => "Flags";

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.Container.Flag value)
        {
            if (record is IContainer containerRecord)
            {
                containerRecord.Flags = value;
            }
        }

        public override Mutagen.Bethesda.Skyrim.Container.Flag GetValue(IMajorRecordGetter record)
        {
            if (record is IContainerGetter containerRecord)
            {
                return containerRecord.Flags;
            }
            return default(Mutagen.Bethesda.Skyrim.Container.Flag);
        }

        protected override Mutagen.Bethesda.Skyrim.Container.Flag[] GetAllFlags()
        {
            return Enum.GetValues<Mutagen.Bethesda.Skyrim.Container.Flag>();
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.Container.Flag flags, Mutagen.Bethesda.Skyrim.Container.Flag flag)
        {
            return flags.HasFlag(flag);
        }

        protected override Mutagen.Bethesda.Skyrim.Container.Flag SetFlag(Mutagen.Bethesda.Skyrim.Container.Flag flags, Mutagen.Bethesda.Skyrim.Container.Flag flag, bool value)
        {
            return value ? flags | flag : flags & ~flag;
        }
    }
}