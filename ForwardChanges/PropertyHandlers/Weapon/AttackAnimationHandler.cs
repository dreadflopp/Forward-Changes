using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class AttackAnimationHandler : AbstractPropertyHandler<WeaponData.AttackAnimationType>
    {
        public override string PropertyName => "AttackAnimation";

        public override void SetValue(IMajorRecord record, WeaponData.AttackAnimationType value)
        {
            var weapon = TryCastRecord<IWeapon>(record, PropertyName);
            if (weapon != null && weapon.Data != null)
            {
                weapon.Data.AttackAnimation = value;
            }
        }

        public override WeaponData.AttackAnimationType GetValue(IMajorRecordGetter record)
        {
            var weapon = TryCastRecord<IWeaponGetter>(record, PropertyName);
            if (weapon != null && weapon.Data != null)
            {
                return weapon.Data.AttackAnimation;
            }
            return default;
        }
    }
}