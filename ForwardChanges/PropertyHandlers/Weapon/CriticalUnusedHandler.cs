using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class CriticalUnusedHandler : AbstractPropertyHandler<short>
    {
        public override string PropertyName => "CriticalUnused";

        public override void SetValue(IMajorRecord record, short value)
        {
            var weapon = TryCastRecord<IWeapon>(record, PropertyName);
            if (weapon != null && weapon.Critical != null)
            {
                weapon.Critical.Unused = value;
            }
        }

        public override short GetValue(IMajorRecordGetter record)
        {
            var weapon = TryCastRecord<IWeaponGetter>(record, PropertyName);
            if (weapon != null && weapon.Critical != null)
            {
                return weapon.Critical.Unused;
            }
            return 0;
        }
    }
}