using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Worldspace
{
    public class MajorFlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Worldspace.MajorFlag>
    {
        public override string PropertyName => "MajorFlags";

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.Worldspace.MajorFlag value)
        {
            if (record is IWorldspace worldspace)
            {
                worldspace.MajorFlags = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IWorldspace for {PropertyName}");
            }
        }

        public override Mutagen.Bethesda.Skyrim.Worldspace.MajorFlag GetValue(IMajorRecordGetter record)
        {
            if (record is IWorldspaceGetter worldspace)
            {
                return worldspace.MajorFlags;
            }
            return default(Mutagen.Bethesda.Skyrim.Worldspace.MajorFlag);
        }

        protected override Mutagen.Bethesda.Skyrim.Worldspace.MajorFlag[] GetAllFlags()
        {
            return Enum.GetValues<Mutagen.Bethesda.Skyrim.Worldspace.MajorFlag>();
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.Worldspace.MajorFlag flags, Mutagen.Bethesda.Skyrim.Worldspace.MajorFlag flag)
        {
            return (flags & flag) == flag;
        }

        protected override Mutagen.Bethesda.Skyrim.Worldspace.MajorFlag SetFlag(Mutagen.Bethesda.Skyrim.Worldspace.MajorFlag flags, Mutagen.Bethesda.Skyrim.Worldspace.MajorFlag flag, bool value)
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

