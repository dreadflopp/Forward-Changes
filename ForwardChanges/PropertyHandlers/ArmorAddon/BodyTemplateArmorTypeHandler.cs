using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ArmorAddon
{
    public class BodyTemplateArmorTypeHandler : AbstractPropertyHandler<ArmorType>
    {
        public override string PropertyName => "BodyTemplateArmorType";

        public override void SetValue(IMajorRecord record, ArmorType value)
        {
            var armorAddonRecord = TryCastRecord<IArmorAddon>(record, PropertyName);
            if (armorAddonRecord?.BodyTemplate != null)
            {
                armorAddonRecord.BodyTemplate.ArmorType = value;
            }
        }

        public override ArmorType GetValue(IMajorRecordGetter record)
        {
            var armorAddonRecord = TryCastRecord<IArmorAddonGetter>(record, PropertyName);
            return armorAddonRecord?.BodyTemplate?.ArmorType ?? ArmorType.LightArmor;
        }
    }
}