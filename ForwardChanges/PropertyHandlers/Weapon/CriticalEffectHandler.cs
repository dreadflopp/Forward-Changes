using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class CriticalEffectHandler : AbstractPropertyHandler<IFormLinkGetter<ISpellGetter>>
    {
        public override string PropertyName => "Effect";

        public override void SetValue(IMajorRecord record, IFormLinkGetter<ISpellGetter>? value)
        {
            var weaponRecord = TryCastRecord<IWeapon>(record, PropertyName);
            if (weaponRecord != null && weaponRecord.Critical != null)
            {
                if (value == null || value.FormKey.IsNull)
                {
                    weaponRecord.Critical.Effect.Clear();
                }
                else
                {
                    weaponRecord.Critical.Effect = new FormLink<ISpellGetter>(value.FormKey);
                }
            }
        }

        public override IFormLinkGetter<ISpellGetter>? GetValue(IMajorRecordGetter record)
        {
            var weaponRecord = TryCastRecord<IWeaponGetter>(record, PropertyName);
            if (weaponRecord != null && weaponRecord.Critical != null)
            {
                return weaponRecord.Critical.Effect;
            }
            return null;
        }

        public override bool AreValuesEqual(IFormLinkGetter<ISpellGetter>? value1, IFormLinkGetter<ISpellGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.FormKey.Equals(value2.FormKey);
        }
    }
}