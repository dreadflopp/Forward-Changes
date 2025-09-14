using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class MajorFlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Npc.MajorFlag>
    {
        public override string PropertyName => "MajorFlags";

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.Npc.MajorFlag value)
        {
            if (record is INpc npc)
            {
                npc.MajorFlags = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement INpc for {PropertyName}");
            }
        }

        public override Mutagen.Bethesda.Skyrim.Npc.MajorFlag GetValue(IMajorRecordGetter record)
        {
            if (record is INpcGetter npc)
            {
                return npc.MajorFlags;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement INpcGetter for {PropertyName}");
            }
            return default(Mutagen.Bethesda.Skyrim.Npc.MajorFlag);
        }

        protected override Mutagen.Bethesda.Skyrim.Npc.MajorFlag[] GetAllFlags()
        {
            return Enum.GetValues<Mutagen.Bethesda.Skyrim.Npc.MajorFlag>();
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.Npc.MajorFlag flags, Mutagen.Bethesda.Skyrim.Npc.MajorFlag flag)
        {
            return flags.HasFlag(flag);
        }

        protected override Mutagen.Bethesda.Skyrim.Npc.MajorFlag SetFlag(Mutagen.Bethesda.Skyrim.Npc.MajorFlag flags, Mutagen.Bethesda.Skyrim.Npc.MajorFlag flag, bool value)
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

