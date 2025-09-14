using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Ingredient
{
    public class FlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Ingredient.Flag>
    {
        public override string PropertyName => "Flags";

        public override Mutagen.Bethesda.Skyrim.Ingredient.Flag GetValue(IMajorRecordGetter record)
        {
            if (record is IIngredientGetter ingredientRecord)
            {
                return ingredientRecord.Flags;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IIngredientGetter for {PropertyName}");
            }
            return default(Mutagen.Bethesda.Skyrim.Ingredient.Flag);
        }

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.Ingredient.Flag value)
        {
            if (record is IIngredient ingredientRecord)
            {
                ingredientRecord.Flags = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IIngredient for {PropertyName}");
            }
        }

        protected override Mutagen.Bethesda.Skyrim.Ingredient.Flag[] GetAllFlags()
        {
            return Enum.GetValues<Mutagen.Bethesda.Skyrim.Ingredient.Flag>();
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.Ingredient.Flag flags, Mutagen.Bethesda.Skyrim.Ingredient.Flag flag)
        {
            return (flags & flag) == flag;
        }

        protected override Mutagen.Bethesda.Skyrim.Ingredient.Flag SetFlag(Mutagen.Bethesda.Skyrim.Ingredient.Flag flags, Mutagen.Bethesda.Skyrim.Ingredient.Flag flag, bool value)
        {
            if (value)
            {
                return flags | flag;
            }
            else
            {
                return flags & ~flag;
            }
        }
    }
}