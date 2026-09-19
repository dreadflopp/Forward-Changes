using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim.Assets;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.Interfaces;

namespace DreadsMashedPatch.PropertyHandlers.Worldspace
{
    public class HdLodDiffuseTextureHandler : AbstractPropertyHandler<AssetLinkGetter<SkyrimTextureAssetType>?>
    {
        public override string PropertyName => "HdLodDiffuseTexture";

        public override void SetValue(IMajorRecord record, AssetLinkGetter<SkyrimTextureAssetType>? value)
        {
            var worldspaceRecord = TryCastRecord<IWorldspace>(record, PropertyName);
            if (worldspaceRecord != null)
            {
                if (value != null)
                {
                    worldspaceRecord.HdLodDiffuseTexture = new AssetLink<SkyrimTextureAssetType>(DreadsMashedPatch.PropertyHandlers.General.TexturePathHelper.Normalize(value));
                }
                else
                {
                    worldspaceRecord.HdLodDiffuseTexture = null;
                }
            }
        }

        public override AssetLinkGetter<SkyrimTextureAssetType>? GetValue(IMajorRecordGetter record)
        {
            var worldspaceRecord = TryCastRecord<IWorldspaceGetter>(record, PropertyName);
            if (worldspaceRecord != null)
            {
                return worldspaceRecord.HdLodDiffuseTexture;
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
