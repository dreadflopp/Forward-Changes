using System;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.DialogResponse
{
    public class FlagsHandler : AbstractFlagPropertyHandler<DialogResponses.Flag>
    {
        public override string PropertyName => "Flags";

        public override DialogResponses.Flag GetValue(IMajorRecordGetter record)
        {
            if (record is IDialogResponsesGetter dialogResponseRecord)
            {
                return dialogResponseRecord.Flags?.Flags ?? default(DialogResponses.Flag);
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IDialogResponsesGetter for {PropertyName}");
            }
            return default(DialogResponses.Flag);
        }

        public override void SetValue(IMajorRecord record, DialogResponses.Flag value)
        {
            if (record is IDialogResponses dialogResponseRecord)
            {
                // Ensure Flags object exists
                if (dialogResponseRecord.Flags == null)
                {
                    dialogResponseRecord.Flags = new DialogResponseFlags();
                }

                dialogResponseRecord.Flags.Flags = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IDialogResponses for {PropertyName}");
            }
        }

        protected override DialogResponses.Flag[] GetAllFlags()
        {
            return Enum.GetValues<DialogResponses.Flag>();
        }

        protected override bool IsFlagSet(DialogResponses.Flag flags, DialogResponses.Flag flag)
        {
            return (flags & flag) == flag;
        }

        protected override DialogResponses.Flag SetFlag(DialogResponses.Flag flags, DialogResponses.Flag flag, bool value)
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