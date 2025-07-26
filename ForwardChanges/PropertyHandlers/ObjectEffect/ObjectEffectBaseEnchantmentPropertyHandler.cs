using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ObjectEffect
{
    public class ObjectEffectBaseEnchantmentPropertyHandler : AbstractPropertyHandler<IFormLinkNullableGetter<IObjectEffectGetter>>
    {
        public override string PropertyName => "BaseEnchantment";

        public override void SetValue(IMajorRecord record, IFormLinkNullableGetter<IObjectEffectGetter>? value)
        {
            if (record is IObjectEffect objectEffectRecord)
            {
                objectEffectRecord.BaseEnchantment = new FormLinkNullable<IObjectEffectGetter>(value?.FormKey ?? FormKey.Null);
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IObjectEffect for {PropertyName}");
            }
        }

        public override IFormLinkNullableGetter<IObjectEffectGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IObjectEffectGetter objectEffectRecord)
            {
                // Convert IFormLinkGetter to IFormLinkNullableGetter
                return objectEffectRecord.BaseEnchantment as IFormLinkNullableGetter<IObjectEffectGetter>;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IObjectEffectGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(IFormLinkNullableGetter<IObjectEffectGetter>? value1, IFormLinkNullableGetter<IObjectEffectGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.FormKey.Equals(value2.FormKey);
        }
    }
}