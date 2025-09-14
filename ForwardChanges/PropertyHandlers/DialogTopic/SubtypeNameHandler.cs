using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.DialogTopic
{
    public class SubtypeNameHandler : AbstractPropertyHandler<RecordType>
    {
        public override string PropertyName => "SubtypeName";

        public override RecordType GetValue(IMajorRecordGetter record)
        {
            if (record is IDialogTopicGetter dialogTopicRecord)
            {
                return dialogTopicRecord.SubtypeName;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IDialogTopicGetter for {PropertyName}");
            }
            return default(RecordType);
        }

        public override void SetValue(IMajorRecord record, RecordType value)
        {
            if (record is IDialogTopic dialogTopicRecord)
            {
                dialogTopicRecord.SubtypeName = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IDialogTopic for {PropertyName}");
            }
        }
    }
}