using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class RumbleDurationHandler : AbstractPropertyHandler<float>
    {
        public override string PropertyName => "RumbleDuration";

        public override void SetValue(IMajorRecord record, float value)
        {
            var weapon = TryCastRecord<IWeapon>(record, PropertyName);
            if (weapon != null && weapon.Data != null)
            {
                weapon.Data.RumbleDuration = value;
            }
        }

        public override float GetValue(IMajorRecordGetter record)
        {
            var weapon = TryCastRecord<IWeaponGetter>(record, PropertyName);
            if (weapon != null && weapon.Data != null)
            {
                return weapon.Data.RumbleDuration;
            }
            return 0f;
        }
    }
}