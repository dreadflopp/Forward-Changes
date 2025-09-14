using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Ingestible
{
    public class AddictionHandler : AbstractPropertyHandler<IFormLinkGetter<ISkyrimMajorRecordGetter>>
    {
        public override string PropertyName => "Addiction";

        public override IFormLinkGetter<ISkyrimMajorRecordGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IIngestibleGetter ingestibleRecord)
            {
                return ingestibleRecord.Addiction;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IIngestibleGetter for {PropertyName}");
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, IFormLinkGetter<ISkyrimMajorRecordGetter>? value)
        {
            if (record is IIngestible ingestibleRecord)
            {
                if (value != null && !value.FormKey.IsNull)
                {
                    ingestibleRecord.Addiction = new FormLink<ISkyrimMajorRecordGetter>(value.FormKey);
                }
                else
                {
                    ingestibleRecord.Addiction.Clear();
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IIngestible for {PropertyName}");
            }
        }

        public override bool AreValuesEqual(IFormLinkGetter<ISkyrimMajorRecordGetter>? value1, IFormLinkGetter<ISkyrimMajorRecordGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            return value1.FormKey.Equals(value2.FormKey);
        }
    }
}