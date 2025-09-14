using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class CriticalVersioningHandler : AbstractPropertyHandler<CriticalData.VersioningBreaks>
    {
        public override string PropertyName => "Versioning";

        public override void SetValue(IMajorRecord record, CriticalData.VersioningBreaks value)
        {
            var weapon = TryCastRecord<IWeapon>(record, PropertyName);
            if (weapon != null && weapon.Critical != null)
            {
                weapon.Critical.Versioning = value;
            }
        }

        public override CriticalData.VersioningBreaks GetValue(IMajorRecordGetter record)
        {
            var weapon = TryCastRecord<IWeaponGetter>(record, PropertyName);
            if (weapon != null && weapon.Critical != null)
            {
                return weapon.Critical.Versioning;
            }
            return default;
        }
    }
}