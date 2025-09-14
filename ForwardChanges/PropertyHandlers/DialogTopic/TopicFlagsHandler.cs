using System;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.DialogTopic
{
    public class TopicFlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.DialogTopic.TopicFlag>
    {
        public override string PropertyName => "TopicFlags";

        public override Mutagen.Bethesda.Skyrim.DialogTopic.TopicFlag GetValue(IMajorRecordGetter record)
        {
            if (record is IDialogTopicGetter dialogTopicRecord)
            {
                return dialogTopicRecord.TopicFlags;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IDialogTopicGetter for {PropertyName}");
            }
            return default(Mutagen.Bethesda.Skyrim.DialogTopic.TopicFlag);
        }

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.DialogTopic.TopicFlag value)
        {
            if (record is IDialogTopic dialogTopicRecord)
            {
                dialogTopicRecord.TopicFlags = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IDialogTopic for {PropertyName}");
            }
        }

        protected override Mutagen.Bethesda.Skyrim.DialogTopic.TopicFlag[] GetAllFlags()
        {
            return Enum.GetValues<Mutagen.Bethesda.Skyrim.DialogTopic.TopicFlag>();
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.DialogTopic.TopicFlag flags, Mutagen.Bethesda.Skyrim.DialogTopic.TopicFlag flag)
        {
            return (flags & flag) == flag;
        }

        protected override Mutagen.Bethesda.Skyrim.DialogTopic.TopicFlag SetFlag(Mutagen.Bethesda.Skyrim.DialogTopic.TopicFlag flags, Mutagen.Bethesda.Skyrim.DialogTopic.TopicFlag flag, bool value)
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