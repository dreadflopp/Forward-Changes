using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim.Assets;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Worldspace
{
    public class MapImageHandler : AbstractPropertyHandler<AssetLinkGetter<SkyrimTextureAssetType>?>
    {
        public override string PropertyName => "MapImage";

        public override void SetValue(IMajorRecord record, AssetLinkGetter<SkyrimTextureAssetType>? value)
        {
            var worldspaceRecord = TryCastRecord<IWorldspace>(record, PropertyName);
            if (worldspaceRecord != null)
            {
                if (value != null)
                {
                    worldspaceRecord.MapImage = new AssetLink<SkyrimTextureAssetType>(ForwardChanges.PropertyHandlers.General.TexturePathHelper.Normalize(value));
                }
                else
                {
                    worldspaceRecord.MapImage = null;
                }
            }
        }

        public override AssetLinkGetter<SkyrimTextureAssetType>? GetValue(IMajorRecordGetter record)
        {
            var worldspaceRecord = TryCastRecord<IWorldspaceGetter>(record, PropertyName);
            if (worldspaceRecord != null)
            {
                return worldspaceRecord.MapImage;
            }
            return null;
        }

        public override bool AreValuesEqual(AssetLinkGetter<SkyrimTextureAssetType>? value1, AssetLinkGetter<SkyrimTextureAssetType>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return ForwardChanges.PropertyHandlers.General.TexturePathHelper.AreEqual(value1, value2);
        }

        public override string FormatValue(object? value)
        {
            if (value is not AssetLinkGetter<SkyrimTextureAssetType> assetLink)
            {
                return value?.ToString() ?? "null";
            }

            return ForwardChanges.PropertyHandlers.General.TexturePathHelper.Normalize(assetLink);
        }
    }
}
