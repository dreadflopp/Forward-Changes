using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class DetectionSoundLevelHandler : AbstractPropertyHandler<SoundLevel?>
    {
        public override string PropertyName => "DetectionSoundLevel";

        public override void SetValue(IMajorRecord record, SoundLevel? value)
        {
            var weaponRecord = TryCastRecord<IWeapon>(record, PropertyName);
            if (weaponRecord != null)
            {
                weaponRecord.DetectionSoundLevel = value;
            }
        }

        public override SoundLevel? GetValue(IMajorRecordGetter record)
        {
            var weaponRecord = TryCastRecord<IWeaponGetter>(record, PropertyName);
            if (weaponRecord != null)
            {
                return weaponRecord.DetectionSoundLevel;
            }
            return null;
        }
    }
}