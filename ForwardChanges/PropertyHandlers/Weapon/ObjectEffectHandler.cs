using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class ObjectEffectHandler : AbstractFormLinkPropertyHandler<IWeapon, IWeaponGetter, IEffectRecordGetter>
    {
        public override string PropertyName => "ObjectEffect";

        protected override IFormLinkNullableGetter<IEffectRecordGetter>? GetFormLinkValue(IWeaponGetter record)
        {
            return record.ObjectEffect;
        }

        protected override void SetFormLinkValue(IWeapon record, IFormLinkNullableGetter<IEffectRecordGetter>? value)
        {
            if (value == null)
            {
                record.ObjectEffect.Clear();
            }
            else
            {
                record.ObjectEffect = new FormLinkNullable<IEffectRecordGetter>(value.FormKey);
            }
        }
    }
}