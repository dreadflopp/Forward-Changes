using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Ingredient
{
    public class IngredientValueHandler : AbstractPropertyHandler<int?>
    {
        public override string PropertyName => "IngredientValue";

        public override void SetValue(IMajorRecord record, int? value)
        {
            var ingredientRecord = TryCastRecord<IIngredient>(record, PropertyName);
            if (ingredientRecord != null)
            {
                ingredientRecord.IngredientValue = value ?? 0;
            }
        }

        public override int? GetValue(IMajorRecordGetter record)
        {
            var ingredientRecord = TryCastRecord<IIngredientGetter>(record, PropertyName);
            if (ingredientRecord != null)
            {
                return ingredientRecord.IngredientValue;
            }
            return null;
        }


    }
}

