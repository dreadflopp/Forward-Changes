using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Ingredient
{
    public class IconsHandler : AbstractIconsHandler<IIngredientGetter, IIngredient>
    {
        protected override IIconsGetter? GetIcons(IIngredientGetter record)
        {
            return record.Icons;
        }

        protected override void SetIcons(IIngredient record, Icons? value)
        {
            record.Icons = value;
        }
    }
}