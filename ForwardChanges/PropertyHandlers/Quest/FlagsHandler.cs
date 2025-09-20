using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Quest
{
    public class FlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Quest.Flag>
    {
        public override string PropertyName => "Flags";

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.Quest.Flag value)
        {
            if (record is IQuest questRecord)
            {
                questRecord.Flags = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IQuest for {PropertyName}");
            }
        }

        public override Mutagen.Bethesda.Skyrim.Quest.Flag GetValue(IMajorRecordGetter record)
        {
            if (record is IQuestGetter questRecord)
            {
                return questRecord.Flags;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IQuestGetter for {PropertyName}");
            }
            return (Mutagen.Bethesda.Skyrim.Quest.Flag)0;
        }

        protected override Mutagen.Bethesda.Skyrim.Quest.Flag[] GetAllFlags()
        {
            return Enum.GetValues<Mutagen.Bethesda.Skyrim.Quest.Flag>();
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.Quest.Flag flags, Mutagen.Bethesda.Skyrim.Quest.Flag flag)
        {
            return flags.HasFlag(flag);
        }

        protected override Mutagen.Bethesda.Skyrim.Quest.Flag SetFlag(Mutagen.Bethesda.Skyrim.Quest.Flag flags, Mutagen.Bethesda.Skyrim.Quest.Flag flag, bool value)
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
