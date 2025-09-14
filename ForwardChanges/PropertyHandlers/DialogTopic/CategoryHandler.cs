using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.DialogTopic
{
    public class CategoryHandler : AbstractPropertyHandler<Mutagen.Bethesda.Skyrim.DialogTopic.CategoryEnum>
    {
        public override string PropertyName => "Category";

        public override Mutagen.Bethesda.Skyrim.DialogTopic.CategoryEnum GetValue(IMajorRecordGetter record)
        {
            if (record is IDialogTopicGetter dialogTopicRecord)
            {
                return dialogTopicRecord.Category;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IDialogTopicGetter for {PropertyName}");
            }
            return default(Mutagen.Bethesda.Skyrim.DialogTopic.CategoryEnum);
        }

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.DialogTopic.CategoryEnum value)
        {
            if (record is IDialogTopic dialogTopicRecord)
            {
                dialogTopicRecord.Category = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IDialogTopic for {PropertyName}");
            }
        }
    }
}