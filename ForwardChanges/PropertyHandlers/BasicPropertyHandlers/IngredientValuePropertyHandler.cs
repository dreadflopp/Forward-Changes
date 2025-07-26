using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class IngredientValuePropertyHandler : AbstractPropertyHandler<int?>
    {
        public override string PropertyName => "IngredientValue";

        public override void SetValue(IMajorRecord record, int? value)
        {
            if (record is IIngredient ingredientRecord)
            {
                ingredientRecord.IngredientValue = value ?? 0;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IIngredient for {PropertyName}");
            }
        }

        public override int? GetValue(IMajorRecordGetter record)
        {
            if (record is IIngredientGetter ingredientRecord)
            {
                return ingredientRecord.IngredientValue;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IIngredientGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(int? value1, int? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.Value == value2.Value;
        }
    }
}