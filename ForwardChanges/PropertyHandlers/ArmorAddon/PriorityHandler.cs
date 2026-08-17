using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using Noggog;

namespace ForwardChanges.PropertyHandlers.ArmorAddon
{
    public class PriorityHandler : AbstractPropertyHandler<IGenderedItemGetter<byte>>
    {
        public override string PropertyName => "Priority";

        public override void SetValue(IMajorRecord record, IGenderedItemGetter<byte>? value)
        {
            var armorAddonRecord = TryCastRecord<IArmorAddon>(record, PropertyName);
            if (armorAddonRecord != null)
            {
                if (value == null)
                {
                    // Priority might not accept null, create a default gendered item
                    armorAddonRecord.Priority = new GenderedItem<byte>(0, 0);
                }
                else
                {
                    // Create a new GenderedItem with the values from the getter
                    var newGenderedItem = new GenderedItem<byte>(value.Male, value.Female);
                    armorAddonRecord.Priority = newGenderedItem;
                }
            }
        }

        public override IGenderedItemGetter<byte>? GetValue(IMajorRecordGetter record)
        {
            var armorAddonRecord = TryCastRecord<IArmorAddonGetter>(record, PropertyName);
            if (armorAddonRecord != null)
            {
                return armorAddonRecord.Priority;
            }
            return null;
        }

        public override bool AreValuesEqual(IGenderedItemGetter<byte>? value1, IGenderedItemGetter<byte>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.Male == value2.Male && value1.Female == value2.Female;
        }

        public override string FormatValue(object? value)
        {
            if (value is IGenderedItemGetter<byte> gendered)
            {
                return $"Male: {gendered.Male}, Female: {gendered.Female}";
            }
            return value?.ToString() ?? "null";
        }
    }
}