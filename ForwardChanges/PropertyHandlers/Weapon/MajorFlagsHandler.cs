using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class MajorFlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Weapon.MajorFlag>
    {
        public override string PropertyName => "MajorFlags";

        public override Mutagen.Bethesda.Skyrim.Weapon.MajorFlag GetValue(IMajorRecordGetter record)
        {
            if (record is IWeaponGetter weapon)
            {
                return weapon.MajorFlags;
            }
            return default;
        }

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.Weapon.MajorFlag value)
        {
            if (record is IWeapon weapon)
            {
                weapon.MajorFlags = value;
            }
        }


        protected override Mutagen.Bethesda.Skyrim.Weapon.MajorFlag[] GetAllFlags()
        {
            return Enum.GetValues<Mutagen.Bethesda.Skyrim.Weapon.MajorFlag>();
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.Weapon.MajorFlag flags, Mutagen.Bethesda.Skyrim.Weapon.MajorFlag flag)
        {
            return (flags & flag) == flag;
        }

        protected override Mutagen.Bethesda.Skyrim.Weapon.MajorFlag SetFlag(Mutagen.Bethesda.Skyrim.Weapon.MajorFlag flags, Mutagen.Bethesda.Skyrim.Weapon.MajorFlag flag, bool value)
        {
            if (value)
                return flags | flag;
            else
                return flags & ~flag;
        }
    }
}
