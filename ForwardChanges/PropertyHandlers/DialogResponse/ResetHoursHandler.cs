using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.DialogResponse
{
    public class ResetHoursHandler : AbstractPropertyHandler<float>
    {
        public override string PropertyName => "ResetHours";

        public override float GetValue(IMajorRecordGetter record)
        {
            if (record is IDialogResponsesGetter dialogResponseRecord)
            {
                return dialogResponseRecord.Flags?.ResetHours ?? 0f;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IDialogResponsesGetter for {PropertyName}");
            }
            return 0f;
        }

        public override void SetValue(IMajorRecord record, float value)
        {
            if (record is IDialogResponses dialogResponseRecord)
            {
                // Ensure Flags object exists
                if (dialogResponseRecord.Flags == null)
                {
                    dialogResponseRecord.Flags = new DialogResponseFlags();
                }

                dialogResponseRecord.Flags.ResetHours = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IDialogResponses for {PropertyName}");
            }
        }

        public override bool AreValuesEqual(float value1, float value2)
        {
            // Use small epsilon for float comparison
            return Math.Abs(value1 - value2) < 0.001f;
        }
    }
}