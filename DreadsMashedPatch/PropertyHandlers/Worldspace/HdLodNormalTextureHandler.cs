using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim.Assets;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.Interfaces;

namespace DreadsMashedPatch.PropertyHandlers.Worldspace
{
    public class HdLodNormalTextureHandler : AbstractPropertyHandler<AssetLinkGetter<SkyrimTextureAssetType>?>
    {
        public override string PropertyName => "HdLodNormalTexture";

        public override void SetValue(IMajorRecord record, AssetLinkGetter<SkyrimTextureAssetType>? value)
        {
            var worldspaceRecord = TryCastRecord<IWorldspace>(record, PropertyName);
            if (worldspaceRecord != null)
            {
                if (value != null)
                {
                    worldspaceRecord.HdLodNormalTexture = new AssetLink<SkyrimTextureAssetType>(DreadsMashedPatch.PropertyHandlers.General.TexturePathHelper.Normalize(value));
                }
                else
                {
                    worldspaceRecord.HdLodNormalTexture = null;
                }
            }
        }

        public override AssetLinkGetter<SkyrimTextureAssetType>? GetValue(IMajorRecordGetter record)
        {
            var worldspaceRecord = TryCastRecord<IWorldspaceGetter>(record, PropertyName);
            if (worldspaceRecord != null)
            {
                return worldspaceRecord.HdLodNormalTexture;
            }
            return null;
        }

        public override bool AreValuesEqual(AssetLinkGetter<SkyrimTextureAssetType>? value1, AssetLinkGetter<SkyrimTextureAssetType>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return DreadsMashedPatch.PropertyHandlers.General.TexturePathHelper.AreEqual(value1, value2);
        }

        public override string FormatValue(object? value)
        {
            if (value is not AssetLinkGetter<SkyrimTextureAssetType> assetLink)
            {
                return value?.ToString() ?? "null";
            }

            return DreadsMashedPatch.PropertyHandlers.General.TexturePathHelper.Normalize(assetLink);
        }
    }
}
