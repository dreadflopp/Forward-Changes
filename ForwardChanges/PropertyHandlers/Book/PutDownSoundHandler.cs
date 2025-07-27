using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Book
{
    public class PutDownSoundHandler : AbstractPropertyHandler<IFormLinkNullableGetter<ISoundDescriptorGetter>>
    {
        public override string PropertyName => "PutDownSound";

        public override void SetValue(IMajorRecord record, IFormLinkNullableGetter<ISoundDescriptorGetter>? value)
        {
            if (record is IBook book)
            {
                if (value != null && !value.FormKey.IsNull)
                {
                    book.PutDownSound = new FormLinkNullable<ISoundDescriptorGetter>(value.FormKey);
                }
                else
                {
                    book.PutDownSound.Clear();
                }
            }
            else
            {
                Console.WriteLine($"Error: Record is not a Book for {PropertyName}");
            }
        }

        public override IFormLinkNullableGetter<ISoundDescriptorGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IBookGetter book)
            {
                return book.PutDownSound;
            }
            else
            {
                Console.WriteLine($"Error: Record is not a Book for {PropertyName}");
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

