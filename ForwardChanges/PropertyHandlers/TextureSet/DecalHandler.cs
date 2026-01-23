using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.TextureSet
{
    public class DecalHandler : AbstractPropertyHandler<IDecalGetter?>
    {
        public override string PropertyName => "Decal";

        public override void SetValue(IMajorRecord record, IDecalGetter? value)
        {
            var tx = TryCastRecord<ITextureSet>(record, PropertyName);
            if (tx != null)
                tx.Decal = value == null ? null : value.DeepCopy();
        }

        public override IDecalGetter? GetValue(IMajorRecordGetter record)
        {
            var tx = TryCastRecord<ITextureSetGetter>(record, PropertyName);
            return tx?.Decal;
        }

        public override bool AreValuesEqual(IDecalGetter? value1, IDecalGetter? value2)
        {
            return Equals(value1, value2);
        }

        public override string FormatValue(object? value) => value?.ToString() ?? "null";
    }
}
