using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim.Assets;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.TextureSet
{
    public class DiffuseHandler : AbstractPropertyHandler<AssetLinkGetter<SkyrimTextureAssetType>?>
    {
        public override string PropertyName => "Diffuse";

        public override void SetValue(IMajorRecord record, AssetLinkGetter<SkyrimTextureAssetType>? value)
        {
            var tx = TryCastRecord<ITextureSet>(record, PropertyName);
            if (tx != null)
            {
                tx.Diffuse = value == null ? null : new AssetLink<SkyrimTextureAssetType>(value.DataRelativePath);
            }
        }

        public override AssetLinkGetter<SkyrimTextureAssetType>? GetValue(IMajorRecordGetter record)
        {
            var tx = TryCastRecord<ITextureSetGetter>(record, PropertyName);
            return tx?.Diffuse;
        }

        public override bool AreValuesEqual(AssetLinkGetter<SkyrimTextureAssetType>? value1, AssetLinkGetter<SkyrimTextureAssetType>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.DataRelativePath == value2.DataRelativePath;
        }

        public override string FormatValue(object? value) =>
            value is AssetLinkGetter<SkyrimTextureAssetType> al ? al.DataRelativePath.ToString() : value?.ToString() ?? "null";
    }
}
