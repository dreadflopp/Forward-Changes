using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class OnHitHandler : AbstractPropertyHandler<WeaponData.OnHitType>
    {
        public override string PropertyName => "OnHit";

        public override void SetValue(IMajorRecord record, WeaponData.OnHitType value)
        {
            var weapon = TryCastRecord<IWeapon>(record, PropertyName);
            if (weapon != null && weapon.Data != null)
            {
                weapon.Data.OnHit = value;
            }
        }

        public override WeaponData.OnHitType GetValue(IMajorRecordGetter record)
        {
            var weapon = TryCastRecord<IWeaponGetter>(record, PropertyName);
            if (weapon != null && weapon.Data != null)
            {
                return weapon.Data.OnHit;
            }
            return default;
        }
    }
}