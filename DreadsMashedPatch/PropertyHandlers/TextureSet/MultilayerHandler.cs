using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim.Assets;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.TextureSet
{
    public class MultilayerHandler : AbstractPropertyHandler<AssetLinkGetter<SkyrimTextureAssetType>?>
    {
        public override string PropertyName => "Multilayer";

        public override void SetValue(IMajorRecord record, AssetLinkGetter<SkyrimTextureAssetType>? value)
        {
            var tx = TryCastRecord<ITextureSet>(record, PropertyName);
            if (tx != null)
                tx.Multilayer = value == null ? null : new AssetLink<SkyrimTextureAssetType>(ForwardChanges.PropertyHandlers.General.TexturePathHelper.Normalize(value));
        }

        public override AssetLinkGetter<SkyrimTextureAssetType>? GetValue(IMajorRecordGetter record)
        {
            var tx = TryCastRecord<ITextureSetGetter>(record, PropertyName);
            return tx?.Multilayer;
        }

        public override bool AreValuesEqual(AssetLinkGetter<SkyrimTextureAssetType>? value1, AssetLinkGetter<SkyrimTextureAssetType>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return ForwardChanges.PropertyHandlers.General.TexturePathHelper.AreEqual(value1, value2);
        }

        public override string FormatValue(object? value) =>
            value is AssetLinkGetter<SkyrimTextureAssetType> al ? ForwardChanges.PropertyHandlers.General.TexturePathHelper.Normalize(al) : value?.ToString() ?? "null";
    }
}

