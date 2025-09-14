using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.DialogTopic
{
    public class SubtypeHandler : AbstractPropertyHandler<Mutagen.Bethesda.Skyrim.DialogTopic.SubtypeEnum>
    {
        public override string PropertyName => "Subtype";

        public override Mutagen.Bethesda.Skyrim.DialogTopic.SubtypeEnum GetValue(IMajorRecordGetter record)
        {
            if (record is IDialogTopicGetter dialogTopicRecord)
            {
                return dialogTopicRecord.Subtype;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IDialogTopicGetter for {PropertyName}");
            }
            return default(Mutagen.Bethesda.Skyrim.DialogTopic.SubtypeEnum);
        }

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.DialogTopic.SubtypeEnum value)
        {
            if (record is IDialogTopic dialogTopicRecord)
            {
                dialogTopicRecord.Subtype = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IDialogTopic for {PropertyName}");
            }
        }
    }
}