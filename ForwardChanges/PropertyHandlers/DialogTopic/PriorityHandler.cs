using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.DialogTopic
{
    public class PriorityHandler : AbstractPropertyHandler<float>
    {
        public override string PropertyName => "Priority";

        public override float GetValue(IMajorRecordGetter record)
        {
            if (record is IDialogTopicGetter dialogTopicRecord)
            {
                return dialogTopicRecord.Priority;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IDialogTopicGetter for {PropertyName}");
            }
            return 0f;
        }

        public override void SetValue(IMajorRecord record, float value)
        {
            if (record is IDialogTopic dialogTopicRecord)
            {
                dialogTopicRecord.Priority = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IDialogTopic for {PropertyName}");
            }
        }

        public override bool AreValuesEqual(float value1, float value2)
        {
            // Use small epsilon for float comparison
            return Math.Abs(value1 - value2) < 0.001f;
        }
    }
}