using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Ingredient
{
    public class PickUpSoundHandler : AbstractFormLinkPropertyHandler<IIngredient, IIngredientGetter, ISoundDescriptorGetter>
    {
        public override string PropertyName => "PickUpSound";

        protected override IFormLinkNullableGetter<ISoundDescriptorGetter>? GetFormLinkValue(IIngredientGetter record)
        {
            return record.PickUpSound;
        }

        protected override void SetFormLinkValue(IIngredient record, IFormLinkNullableGetter<ISoundDescriptorGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.PickUpSound = new FormLinkNullable<ISoundDescriptorGetter>(value.FormKey);
            }
            else
            {
                record.PickUpSound.Clear();
            }
        }
    }
}