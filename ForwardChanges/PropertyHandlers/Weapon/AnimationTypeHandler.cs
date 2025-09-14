using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class AnimationTypeHandler : AbstractPropertyHandler<WeaponAnimationType>
    {
        public override string PropertyName => "AnimationType";

        public override void SetValue(IMajorRecord record, WeaponAnimationType value)
        {
            var weapon = TryCastRecord<IWeapon>(record, PropertyName);
            if (weapon != null && weapon.Data != null)
            {
                weapon.Data.AnimationType = value;
            }
        }

        public override WeaponAnimationType GetValue(IMajorRecordGetter record)
        {
            var weapon = TryCastRecord<IWeaponGetter>(record, PropertyName);
            if (weapon != null && weapon.Data != null)
            {
                return weapon.Data.AnimationType;
            }
            return default;
        }
    }
}