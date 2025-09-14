using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Ingestible
{
    public class ConsumeSoundHandler : AbstractPropertyHandler<IFormLinkGetter<ISoundDescriptorGetter>>
    {
        public override string PropertyName => "ConsumeSound";

        public override IFormLinkGetter<ISoundDescriptorGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IIngestibleGetter ingestibleRecord)
            {
                return ingestibleRecord.ConsumeSound;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IIngestibleGetter for {PropertyName}");
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, IFormLinkGetter<ISoundDescriptorGetter>? value)
        {
            if (record is IIngestible ingestibleRecord)
            {
                if (value != null && !value.FormKey.IsNull)
                {
                    ingestibleRecord.ConsumeSound = new FormLink<ISoundDescriptorGetter>(value.FormKey);
                }
                else
                {
                    ingestibleRecord.ConsumeSound.Clear();
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IIngestible for {PropertyName}");
            }
        }

        public override bool AreValuesEqual(IFormLinkGetter<ISoundDescriptorGetter>? value1, IFormLinkGetter<ISoundDescriptorGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            return value1.FormKey.Equals(value2.FormKey);
        }
    }
}