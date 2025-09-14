using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Worldspace
{
    public class FlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Worldspace.Flag>
    {
        public override string PropertyName => "Flags";

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.Worldspace.Flag value)
        {
            var worldspaceRecord = TryCastRecord<IWorldspace>(record, PropertyName);
            if (worldspaceRecord != null)
            {
                worldspaceRecord.Flags = value;
            }
        }

        public override Mutagen.Bethesda.Skyrim.Worldspace.Flag GetValue(IMajorRecordGetter record)
        {
            var worldspaceRecord = TryCastRecord<IWorldspaceGetter>(record, PropertyName);
            if (worldspaceRecord != null)
            {
                return worldspaceRecord.Flags;
            }
            return default;
        }

        protected override Mutagen.Bethesda.Skyrim.Worldspace.Flag[] GetAllFlags()
        {
            return Enum.GetValues<Mutagen.Bethesda.Skyrim.Worldspace.Flag>();
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.Worldspace.Flag flags, Mutagen.Bethesda.Skyrim.Worldspace.Flag flag)
        {
            return flags.HasFlag(flag);
        }

        protected override Mutagen.Bethesda.Skyrim.Worldspace.Flag SetFlag(Mutagen.Bethesda.Skyrim.Worldspace.Flag flags, Mutagen.Bethesda.Skyrim.Worldspace.Flag flag, bool set)
        {
            if (set)
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