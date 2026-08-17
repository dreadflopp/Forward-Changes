using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim.Assets;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Worldspace
{
    public class WaterEnvironmentMapHandler : AbstractPropertyHandler<AssetLinkGetter<SkyrimTextureAssetType>?>
    {
        public override string PropertyName => "WaterEnvironmentMap";

        public override void SetValue(IMajorRecord record, AssetLinkGetter<SkyrimTextureAssetType>? value)
        {
            var worldspaceRecord = TryCastRecord<IWorldspace>(record, PropertyName);
            if (worldspaceRecord != null)
            {
                if (value != null)
                {
                    // Use DataRelativePath to get the full path including "Data\" prefix
                    worldspaceRecord.WaterEnvironmentMap = new AssetLink<SkyrimTextureAssetType>(ForwardChanges.PropertyHandlers.General.TexturePathHelper.Normalize(value));
                }
                else
                {
                    worldspaceRecord.WaterEnvironmentMap = null;
                }
            }
        }

        public override AssetLinkGetter<SkyrimTextureAssetType>? GetValue(IMajorRecordGetter record)
        {
            var worldspaceRecord = TryCastRecord<IWorldspaceGetter>(record, PropertyName);
            if (worldspaceRecord != null)
            {
                return worldspaceRecord.WaterEnvironmentMap;
            }
            return null;
        }

        public override bool AreValuesEqual(AssetLinkGetter<SkyrimTextureAssetType>? value1, AssetLinkGetter<SkyrimTextureAssetType>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return ForwardChanges.PropertyHandlers.General.TexturePathHelper.Normalize(value1) == ForwardChanges.PropertyHandlers.General.TexturePathHelper.Normalize(value2);
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
