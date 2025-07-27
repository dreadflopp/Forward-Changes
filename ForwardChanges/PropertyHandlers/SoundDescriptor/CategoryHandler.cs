using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.SoundDescriptor
{
    public class CategoryHandler : AbstractPropertyHandler<IFormLinkNullableGetter<ISoundCategoryGetter>>
    {
        public override string PropertyName => "Category";

        public override void SetValue(IMajorRecord record, IFormLinkNullableGetter<ISoundCategoryGetter>? value)
        {
            if (record is ISoundDescriptor soundDescriptor)
            {
                if (value != null)
                {
                    soundDescriptor.Category.SetTo(value.FormKey);
                }
                else
                {
                    soundDescriptor.Category.SetTo(null);
                }
            }
        }

        public override IFormLinkNullableGetter<ISoundCategoryGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is ISoundDescriptorGetter soundDescriptor)
            {
                return soundDescriptor.Category;
            }
            return null;
        }

        public override bool AreValuesEqual(IFormLinkNullableGetter<ISoundCategoryGetter>? value1, IFormLinkNullableGetter<ISoundCategoryGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.FormKey.Equals(value2.FormKey);
        }
    }
}

