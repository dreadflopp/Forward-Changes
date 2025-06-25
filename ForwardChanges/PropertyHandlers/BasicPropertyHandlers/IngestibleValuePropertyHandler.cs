using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class IngestibleValuePropertyHandler : AbstractPropertyHandler<uint?>
    {
        public override string PropertyName => "Value";

        public override void SetValue(IMajorRecord record, uint? value)
        {
            if (record is IIngestible ingestible)
            {
                ingestible.Value = value ?? 0u;
            }
            else
            {
                Console.WriteLine($"Error: Record is not an Ingestible for {PropertyName}");
            }
        }

        public override uint? GetValue(IMajorRecordGetter record)
        {
            if (record is IIngestibleGetter ingestible)
            {
                return ingestible.Value;
            }
            else
            {
                Console.WriteLine($"Error: Record is not an Ingestible for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(uint? value1, uint? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.Value == value2.Value;
        }
    }
}