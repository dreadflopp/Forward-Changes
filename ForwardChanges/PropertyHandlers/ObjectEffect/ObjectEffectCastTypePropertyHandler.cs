using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ObjectEffect
{
    public class ObjectEffectCastTypePropertyHandler : AbstractPropertyHandler<CastType?>
    {
        public override string PropertyName => "CastType";

        public override void SetValue(IMajorRecord record, CastType? value)
        {
            if (record is IObjectEffect objectEffectRecord)
            {
                objectEffectRecord.CastType = value ?? CastType.ConstantEffect;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IObjectEffect for {PropertyName}");
            }
        }

        public override CastType? GetValue(IMajorRecordGetter record)
        {
            if (record is IObjectEffectGetter objectEffectRecord)
            {
                return objectEffectRecord.CastType;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IObjectEffectGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(CastType? value1, CastType? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.Value == value2.Value;
        }
    }
}