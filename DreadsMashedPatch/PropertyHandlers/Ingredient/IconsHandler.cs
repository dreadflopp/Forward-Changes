using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using DreadsMashedPatch.PropertyHandlers.Abstracts;

namespace DreadsMashedPatch.PropertyHandlers.Ingredient
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