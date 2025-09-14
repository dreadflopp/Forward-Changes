using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class CriticalFlagsHandler : AbstractFlagPropertyHandler<CriticalData.Flag>
    {
        public override string PropertyName => "CriticalFlags";

        public override void SetValue(IMajorRecord record, CriticalData.Flag value)
        {
            var weapon = TryCastRecord<IWeapon>(record, PropertyName);
            if (weapon != null && weapon.Critical != null)
            {
                weapon.Critical.Flags = value;
            }
        }

        public override CriticalData.Flag GetValue(IMajorRecordGetter record)
        {
            var weapon = TryCastRecord<IWeaponGetter>(record, PropertyName);
            if (weapon != null && weapon.Critical != null)
            {
                return weapon.Critical.Flags;
            }
            return default;
        }

        protected override CriticalData.Flag[] GetAllFlags()
        {
            return Enum.GetValues<CriticalData.Flag>();
        }

        protected override bool IsFlagSet(CriticalData.Flag flags, CriticalData.Flag flag)
        {
            return (flags & flag) == flag;
        }

        protected override CriticalData.Flag SetFlag(CriticalData.Flag flags, CriticalData.Flag flag, bool value)
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

        protected override string FormatFlag(CriticalData.Flag flag)
        {
            return flag.ToString();
        }
    }
}