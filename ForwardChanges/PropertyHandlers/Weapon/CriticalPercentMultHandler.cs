using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class CriticalPercentMultHandler : AbstractPropertyHandler<float>
    {
        public override string PropertyName => "PercentMult";

        public override void SetValue(IMajorRecord record, float value)
        {
            var weapon = TryCastRecord<IWeapon>(record, PropertyName);
            if (weapon != null && weapon.Critical != null)
            {
                weapon.Critical.PercentMult = value;
            }
        }

        public override float GetValue(IMajorRecordGetter record)
        {
            var weapon = TryCastRecord<IWeaponGetter>(record, PropertyName);
            if (weapon != null && weapon.Critical != null)
            {
                return weapon.Critical.PercentMult;
            }
            return 0f;
        }
    }
}