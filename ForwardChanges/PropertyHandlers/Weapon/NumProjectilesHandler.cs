using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class NumProjectilesHandler : AbstractPropertyHandler<byte>
    {
        public override string PropertyName => "NumProjectiles";

        public override void SetValue(IMajorRecord record, byte value)
        {
            var weapon = TryCastRecord<IWeapon>(record, PropertyName);
            if (weapon != null && weapon.Data != null)
            {
                weapon.Data.NumProjectiles = value;
            }
        }

        public override byte GetValue(IMajorRecordGetter record)
        {
            var weapon = TryCastRecord<IWeaponGetter>(record, PropertyName);
            if (weapon != null && weapon.Data != null)
            {
                return weapon.Data.NumProjectiles;
            }
            return 0;
        }
    }
}