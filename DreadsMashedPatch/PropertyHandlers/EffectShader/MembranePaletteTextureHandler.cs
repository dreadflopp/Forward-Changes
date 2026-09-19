using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim.Assets;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using DreadsMashedPatch.PropertyHandlers.General;

namespace DreadsMashedPatch.PropertyHandlers.EffectShader
{
    public class MembranePaletteTextureHandler : AbstractPropertyHandler<AssetLinkGetter<SkyrimTextureAssetType>>
    {
        public override string PropertyName => "MembranePaletteTexture";

        public override void SetValue(IMajorRecord record, AssetLinkGetter<SkyrimTextureAssetType>? value)
        {
            var effectShader = TryCastRecord<IEffectShader>(record, PropertyName);
            if (effectShader != null)
            {
                if (value != null && !value.IsNull)
                {
                    effectShader.MembranePaletteTexture = new AssetLink<SkyrimTextureAssetType>(EffectShaderTexturePathHelper.Normalize(value));
                }
                else
                {
                    effectShader.MembranePaletteTexture = null;
                }
            }
        }

        public override AssetLinkGetter<SkyrimTextureAssetType>? GetValue(IMajorRecordGetter record)
        {
            var effectShader = TryCastRecord<IEffectShaderGetter>(record, PropertyName);
            if (effectShader != null)
            {
                return effectShader.MembranePaletteTexture;
            }
            return null;
        }

        public override bool AreValuesEqual(AssetLinkGetter<SkyrimTextureAssetType>? value1, AssetLinkGetter<SkyrimTextureAssetType>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return AssetPathHelper.AreEqual(value1, value2);
        }

        public override string FormatValue(object? value)
        {
            if (value is not AssetLinkGetter<SkyrimTextureAssetType> assetLink)
            {
                return value?.ToString() ?? "null";
            }

            return EffectShaderTexturePathHelper.Normalize(assetLink);
        }
    }
}

