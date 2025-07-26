using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using Noggog;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class ArmorAddonWeightSliderEnabledPropertyHandler : AbstractPropertyHandler<IGenderedItemGetter<bool>>
    {
        public override string PropertyName => "WeightSliderEnabled";

        public override void SetValue(IMajorRecord record, IGenderedItemGetter<bool>? value)
        {
            if (record is IArmorAddon armorAddonRecord)
            {
                if (value == null)
                {
                    // WeightSliderEnabled might not accept null, create a default gendered item
                    armorAddonRecord.WeightSliderEnabled = new GenderedItem<bool>(false, false);
                }
                else
                {
                    // Create a new GenderedItem with the values from the getter
                    var newGenderedItem = new GenderedItem<bool>(value.Male, value.Female);
                    armorAddonRecord.WeightSliderEnabled = newGenderedItem;
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IArmorAddon for {PropertyName}");
            }
        }

        public override IGenderedItemGetter<bool>? GetValue(IMajorRecordGetter record)
        {
            if (record is IArmorAddonGetter armorAddonRecord)
            {
                return armorAddonRecord.WeightSliderEnabled;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IArmorAddonGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(IGenderedItemGetter<bool>? value1, IGenderedItemGetter<bool>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            return value1.Male == value2.Male && value1.Female == value2.Female;
        }
    }
}