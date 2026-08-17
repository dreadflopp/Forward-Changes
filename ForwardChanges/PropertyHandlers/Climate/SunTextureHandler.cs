using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Assets;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.General;

namespace ForwardChanges.PropertyHandlers.Climate
{
    public class SunTextureHandler : AbstractPropertyHandler<AssetLinkGetter<SkyrimTextureAssetType>?>
    {
        public override string PropertyName => "SunTexture";

        public override AssetLinkGetter<SkyrimTextureAssetType>? GetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecordGetter record)
        {
            return (record as IClimateGetter)?.SunTexture;
        }

        public override void SetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecord record, AssetLinkGetter<SkyrimTextureAssetType>? value)
        {
            if (record is not IClimate climate)
            {
                return;
            }

            climate.SunTexture = value == null
                ? null
                : new AssetLink<SkyrimTextureAssetType>(TexturePathHelper.Normalize(value));
        }

        public override bool AreValuesEqual(AssetLinkGetter<SkyrimTextureAssetType>? value1, AssetLinkGetter<SkyrimTextureAssetType>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return TexturePathHelper.Normalize(value1) == TexturePathHelper.Normalize(value2);
        }
    }
}
