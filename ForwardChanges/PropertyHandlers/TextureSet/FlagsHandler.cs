using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using TextureSetFlag = Mutagen.Bethesda.Skyrim.TextureSet.Flag;

namespace ForwardChanges.PropertyHandlers.TextureSet
{
    public class FlagsHandler : AbstractPropertyHandler<TextureSetFlag?>
    {
        public override string PropertyName => "Flags";

        public override void SetValue(IMajorRecord record, TextureSetFlag? value)
        {
            var tx = TryCastRecord<ITextureSet>(record, PropertyName);
            if (tx != null)
                tx.Flags = value;
        }

        public override TextureSetFlag? GetValue(IMajorRecordGetter record)
        {
            var tx = TryCastRecord<ITextureSetGetter>(record, PropertyName);
            return tx?.Flags;
        }

        public override bool AreValuesEqual(TextureSetFlag? value1, TextureSetFlag? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1 == value2;
        }

        public override string FormatValue(object? value) => value?.ToString() ?? "null";
    }
}
