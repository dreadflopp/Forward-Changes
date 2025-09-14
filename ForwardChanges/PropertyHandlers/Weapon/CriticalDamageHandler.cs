using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class CriticalDamageHandler : AbstractPropertyHandler<ushort>
    {
        public override string PropertyName => "CriticalDamage";

        public override void SetValue(IMajorRecord record, ushort value)
        {
            var weapon = TryCastRecord<IWeapon>(record, PropertyName);
            if (weapon != null && weapon.Critical != null)
            {
                weapon.Critical.Damage = value;
            }
        }

        public override ushort GetValue(IMajorRecordGetter record)
        {
            var weapon = TryCastRecord<IWeaponGetter>(record, PropertyName);
            if (weapon != null && weapon.Critical != null)
            {
                return weapon.Critical.Damage;
            }
            return 0;
        }
    }
}