using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.SoundDescriptor
{
    public class CategoryHandler : AbstractFormLinkPropertyHandler<ISoundDescriptor, ISoundDescriptorGetter, ISoundCategoryGetter>
    {
        public override string PropertyName => "Category";

        protected override IFormLinkNullableGetter<ISoundCategoryGetter>? GetFormLinkValue(ISoundDescriptorGetter record)
        {
            return record.Category;
        }

        protected override void SetFormLinkValue(ISoundDescriptor record, IFormLinkNullableGetter<ISoundCategoryGetter>? value)
        {
            if (value != null)
            {
                record.Category.SetTo(value.FormKey);
            }
            else
            {
                record.Category.SetTo(null);
            }
        }
    }
}

