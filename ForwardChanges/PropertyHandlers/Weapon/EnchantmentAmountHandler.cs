using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class EnchantmentAmountHandler : AbstractPropertyHandler<ushort?>
    {
        public override string PropertyName => "EnchantmentAmount";

        public override void SetValue(IMajorRecord record, ushort? value)
        {
            var weaponRecord = TryCastRecord<IWeapon>(record, PropertyName);
            if (weaponRecord != null)
            {
                weaponRecord.EnchantmentAmount = value;
            }
        }

        public override ushort? GetValue(IMajorRecordGetter record)
        {
            var weaponRecord = TryCastRecord<IWeaponGetter>(record, PropertyName);
            if (weaponRecord != null)
            {
                return weaponRecord.EnchantmentAmount;
            }
            return null;
        }
    }
}