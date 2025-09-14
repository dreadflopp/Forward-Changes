using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class CriticalUnused4Handler : AbstractPropertyHandler<int>
    {
        public override string PropertyName => "CriticalUnused4";

        public override void SetValue(IMajorRecord record, int value)
        {
            var weapon = TryCastRecord<IWeapon>(record, PropertyName);
            if (weapon != null && weapon.Critical != null)
            {
                weapon.Critical.Unused4 = value;
            }
        }

        public override int GetValue(IMajorRecordGetter record)
        {
            var weapon = TryCastRecord<IWeaponGetter>(record, PropertyName);
            if (weapon != null && weapon.Critical != null)
            {
                return weapon.Critical.Unused4;
            }
            return 0;
        }
    }
}