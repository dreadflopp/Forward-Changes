using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Ingestible
{
    public class PutDownSoundHandler : AbstractPropertyHandler<IFormLinkNullableGetter<ISoundDescriptorGetter>>
    {
        public override string PropertyName => "PutDownSound";

        public override void SetValue(IMajorRecord record, IFormLinkNullableGetter<ISoundDescriptorGetter>? value)
        {
            if (record is IIngestible ingestible)
            {
                if (value != null && !value.FormKey.IsNull)
                {
                    ingestible.PutDownSound = new FormLinkNullable<ISoundDescriptorGetter>(value.FormKey);
                }
                else
                {
                    ingestible.PutDownSound.Clear();
                }
            }
            else
            {
                Console.WriteLine($"Error: Record is not an Ingestible for {PropertyName}");
            }
        }

        public override IFormLinkNullableGetter<ISoundDescriptorGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IIngestibleGetter ingestible)
            {
                return ingestible.PutDownSound;
            }
            else
            {
                Console.WriteLine($"Error: Record is not an Ingestible for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(IFormLinkNullableGetter<ISoundDescriptorGetter>? value1, IFormLinkNullableGetter<ISoundDescriptorGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.FormKey.Equals(value2.FormKey);
        }
    }
}

