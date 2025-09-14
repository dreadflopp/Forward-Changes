using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.DialogResponse
{
    public class FavorLevelHandler : AbstractPropertyHandler<FavorLevel?>
    {
        public override string PropertyName => "FavorLevel";

        public override void SetValue(IMajorRecord record, FavorLevel? value)
        {
            if (record is IDialogResponses dialogResponseRecord)
            {
                dialogResponseRecord.FavorLevel = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IDialogResponses for {PropertyName}");
            }
        }

        public override FavorLevel? GetValue(IMajorRecordGetter record)
        {
            if (record is IDialogResponsesGetter dialogResponseRecord)
            {
                return dialogResponseRecord.FavorLevel;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IDialogResponsesGetter for {PropertyName}");
            }
            return null;
        }
    }
}