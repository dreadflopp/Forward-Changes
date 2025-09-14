using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class WeaponDataFlagsHandler : AbstractFlagPropertyHandler<WeaponData.Flag>
    {
        public override string PropertyName => "Flags";

        public override void SetValue(IMajorRecord record, WeaponData.Flag value)
        {
            var weapon = TryCastRecord<IWeapon>(record, PropertyName);
            if (weapon != null && weapon.Data != null)
            {
                weapon.Data.Flags = value;
            }
        }

        public override WeaponData.Flag GetValue(IMajorRecordGetter record)
        {
            var weapon = TryCastRecord<IWeaponGetter>(record, PropertyName);
            if (weapon != null && weapon.Data != null)
            {
                return weapon.Data.Flags;
            }
            return default;
        }

        protected override WeaponData.Flag[] GetAllFlags()
        {
            return Enum.GetValues<WeaponData.Flag>();
        }

        protected override bool IsFlagSet(WeaponData.Flag flags, WeaponData.Flag flag)
        {
            return (flags & flag) == flag;
        }

        protected override WeaponData.Flag SetFlag(WeaponData.Flag flags, WeaponData.Flag flag, bool value)
        {
            if (value)
            {
                return flags | flag;
            }
            else
            {
                return flags & ~flag;
            }
        }

        protected override string FormatFlag(WeaponData.Flag flag)
        {
            return flag.ToString();
        }
    }
}