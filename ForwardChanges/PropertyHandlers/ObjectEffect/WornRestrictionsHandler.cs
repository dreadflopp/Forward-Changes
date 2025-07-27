using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ObjectEffect
{
    public class WornRestrictionsHandler : AbstractPropertyHandler<IFormLinkNullableGetter<IFormListGetter>>
    {
        public override string PropertyName => "WornRestrictions";

        public override void SetValue(IMajorRecord record, IFormLinkNullableGetter<IFormListGetter>? value)
        {
            if (record is IObjectEffect objectEffectRecord)
            {
                objectEffectRecord.WornRestrictions = new FormLinkNullable<IFormListGetter>(value?.FormKey ?? FormKey.Null);
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IObjectEffect for {PropertyName}");
            }
        }

        public override IFormLinkNullableGetter<IFormListGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IObjectEffectGetter objectEffectRecord)
            {
                // Convert IFormLinkGetter to IFormLinkNullableGetter
                return objectEffectRecord.WornRestrictions as IFormLinkNullableGetter<IFormListGetter>;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IObjectEffectGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(IFormLinkNullableGetter<IFormListGetter>? value1, IFormLinkNullableGetter<IFormListGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.FormKey.Equals(value2.FormKey);
        }
    }
}

