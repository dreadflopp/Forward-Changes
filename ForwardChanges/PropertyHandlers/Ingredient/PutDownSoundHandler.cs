using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Ingredient
{
    public class PutDownSoundHandler : AbstractFormLinkPropertyHandler<IIngredient, IIngredientGetter, ISoundDescriptorGetter>
    {
        public override string PropertyName => "PutDownSound";

        protected override IFormLinkNullableGetter<ISoundDescriptorGetter>? GetFormLinkValue(IIngredientGetter record)
        {
            return record.PutDownSound;
        }

        protected override void SetFormLinkValue(IIngredient record, IFormLinkNullableGetter<ISoundDescriptorGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.PutDownSound = new FormLinkNullable<ISoundDescriptorGetter>(value.FormKey);
            }
            else
            {
                record.PutDownSound.Clear();
            }
        }
    }
}