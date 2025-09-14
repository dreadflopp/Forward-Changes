using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ArmorAddon
{
    public class WeaponAdjustHandler : AbstractPropertyHandler<float>
    {
        public override string PropertyName => "WeaponAdjust";

        public override void SetValue(IMajorRecord record, float value)
        {
            var armorAddonRecord = TryCastRecord<IArmorAddon>(record, PropertyName);
            if (armorAddonRecord != null)
            {
                armorAddonRecord.WeaponAdjust = value;
            }
        }

        public override float GetValue(IMajorRecordGetter record)
        {
            var armorAddonRecord = TryCastRecord<IArmorAddonGetter>(record, PropertyName);
            return armorAddonRecord?.WeaponAdjust ?? 0.0f;
        }
    }
}