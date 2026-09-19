using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Ingredient
{
    public class DestructibleHandler : AbstractDestructibleHandler<IIngredientGetter, IIngredient>
    {
        protected override IDestructibleGetter? GetDestructible(IIngredientGetter record)
        {
            return record.Destructible;
        }

        protected override void SetDestructible(IIngredient record, Destructible? value)
        {
            record.Destructible = value;
        }
    }
}