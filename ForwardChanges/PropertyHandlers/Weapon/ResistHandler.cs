using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class ResistHandler : AbstractPropertyHandler<ActorValue>
    {
        public override string PropertyName => "Resist";

        public override void SetValue(IMajorRecord record, ActorValue value)
        {
            var weapon = TryCastRecord<IWeapon>(record, PropertyName);
            if (weapon != null && weapon.Data != null)
            {
                weapon.Data.Resist = value;
            }
        }

        public override ActorValue GetValue(IMajorRecordGetter record)
        {
            var weapon = TryCastRecord<IWeaponGetter>(record, PropertyName);
            if (weapon != null && weapon.Data != null)
            {
                return weapon.Data.Resist;
            }
            return default;
        }
    }
}