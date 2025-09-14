using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Ingredient
{
    public class EquipTypeHandler : AbstractFormLinkPropertyHandler<IIngredient, IIngredientGetter, IEquipTypeGetter>
    {
        public override string PropertyName => "EquipType";

        protected override IFormLinkNullableGetter<IEquipTypeGetter>? GetFormLinkValue(IIngredientGetter record)
        {
            return record.EquipType;
        }

        protected override void SetFormLinkValue(IIngredient record, IFormLinkNullableGetter<IEquipTypeGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.EquipType = new FormLinkNullable<IEquipTypeGetter>(value.FormKey);
            }
            else
            {
                record.EquipType.Clear();
            }
        }
    }
}