using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Assets;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.General;

namespace DreadsMashedPatch.PropertyHandlers.Eyes
{
    public class IconHandler : AbstractPropertyHandler<AssetLinkGetter<SkyrimTextureAssetType>>
    {
        public override string PropertyName => "Icon";

        public override AssetLinkGetter<SkyrimTextureAssetType> GetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecordGetter record)
        {
            if (record is IEyes eyes)
            {
                return eyes.Icon;
            }

            return new AssetLink<SkyrimTextureAssetType>(string.Empty);
        }

        public override void SetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecord record, AssetLinkGetter<SkyrimTextureAssetType>? value)
        {
            if (record is not IEyes eyes || value == null)
            {
                return;
            }

            eyes.Icon = new AssetLink<SkyrimTextureAssetType>(TexturePathHelper.Normalize(value));
        }

        public override bool AreValuesEqual(AssetLinkGetter<SkyrimTextureAssetType>? value1, AssetLinkGetter<SkyrimTextureAssetType>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return TexturePathHelper.AreEqual(value1, value2);
        }
    }
}
