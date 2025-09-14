using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class BaseVATStoHitChanceHandler : AbstractPropertyHandler<byte>
    {
        public override string PropertyName => "BaseVATStoHitChance";

        public override void SetValue(IMajorRecord record, byte value)
        {
            var weapon = TryCastRecord<IWeapon>(record, PropertyName);
            if (weapon != null && weapon.Data != null)
            {
                weapon.Data.BaseVATStoHitChance = value;
            }
        }

        public override byte GetValue(IMajorRecordGetter record)
        {
            var weapon = TryCastRecord<IWeaponGetter>(record, PropertyName);
            if (weapon != null && weapon.Data != null)
            {
                return weapon.Data.BaseVATStoHitChance;
            }
            return 0;
        }
    }
}