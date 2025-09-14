using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class DamageHandler : AbstractPropertyHandler<ushort>
    {
        public override string PropertyName => "Damage";

        public override void SetValue(IMajorRecord record, ushort value)
        {
            var weapon = TryCastRecord<IWeapon>(record, PropertyName);
            if (weapon != null && weapon.BasicStats != null)
            {
                weapon.BasicStats.Damage = value;
            }
        }

        public override ushort GetValue(IMajorRecordGetter record)
        {
            var weapon = TryCastRecord<IWeaponGetter>(record, PropertyName);
            if (weapon != null && weapon.BasicStats != null)
            {
                return weapon.BasicStats.Damage;
            }
            return 0;
        }
    }
}