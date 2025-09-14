using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class Unknown2Handler : AbstractPropertyHandler<int>
    {
        public override string PropertyName => "Unknown2";

        public override void SetValue(IMajorRecord record, int value)
        {
            var weapon = TryCastRecord<IWeapon>(record, PropertyName);
            if (weapon != null && weapon.Data != null)
            {
                weapon.Data.Unknown2 = value;
            }
        }

        public override int GetValue(IMajorRecordGetter record)
        {
            var weapon = TryCastRecord<IWeaponGetter>(record, PropertyName);
            if (weapon != null && weapon.Data != null)
            {
                return weapon.Data.Unknown2;
            }
            return 0;
        }
    }
}