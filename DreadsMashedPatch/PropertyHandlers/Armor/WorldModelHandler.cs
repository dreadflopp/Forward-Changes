using ForwardChanges.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using ForwardChanges.PropertyHandlers.General;

namespace ForwardChanges.PropertyHandlers.Armor
{
    /// <summary>
    /// Handles ARMO's gendered ArmorModel aggregate, including each model and its icons.
    /// </summary>
    public sealed class WorldModelHandler : AbstractPropertyHandler<IGenderedItemGetter<IArmorModelGetter?>?>
    {
        public override string PropertyName => "WorldModel";

        public override IGenderedItemGetter<IArmorModelGetter?>? GetValue(IMajorRecordGetter record)
        {
            return record is IArmorGetter armor ? armor.WorldModel : null;
        }

        public override void SetValue(IMajorRecord record, IGenderedItemGetter<IArmorModelGetter?>? value)
        {
            if (record is not IArmor armor)
            {
                Console.WriteLine($"Error: Record does not implement IArmor for {PropertyName}");
                return;
            }

            armor.WorldModel = value == null
                ? null
                : new GenderedItem<ArmorModel?>(value.Male?.DeepCopy(), value.Female?.DeepCopy());
        }

        public override bool AreValuesEqual(
            IGenderedItemGetter<IArmorModelGetter?>? value1,
            IGenderedItemGetter<IArmorModelGetter?>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            return ModelsEqual(value1.Male, value2.Male)
                && ModelsEqual(value1.Female, value2.Female);
        }

        public override string FormatValue(object? value)
        {
            if (value is not IGenderedItemGetter<IArmorModelGetter?> gendered)
            {
                return value?.ToString() ?? "null";
            }

            return $"Male: {FormatModel(gendered.Male)}, Female: {FormatModel(gendered.Female)}";
        }

        private static bool ModelsEqual(IArmorModelGetter? value1, IArmorModelGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.Equals(value2);
        }

        private static string FormatModel(IArmorModelGetter? value)
        {
            if (value == null) return "null";

            var modelFile = AssetPathHelper.Format(value.Model?.File);
            var largeIcon = AssetPathHelper.Format(value.Icons?.LargeIconFilename);
            var smallIcon = AssetPathHelper.Format(value.Icons?.SmallIconFilename);
            return $"Model(File: {modelFile}, LargeIcon: {largeIcon}, SmallIcon: {smallIcon})";
        }
    }
}
