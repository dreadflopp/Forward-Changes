using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim.Assets;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.PropertyHandlers.General;

namespace ForwardChanges.PropertyHandlers.EffectShader
{
    public class ParticlePaletteTextureHandler : AbstractPropertyHandler<AssetLinkGetter<SkyrimTextureAssetType>>
    {
        public override string PropertyName => "ParticlePaletteTexture";

        public override void SetValue(IMajorRecord record, AssetLinkGetter<SkyrimTextureAssetType>? value)
        {
            var effectShader = TryCastRecord<IEffectShader>(record, PropertyName);
            if (effectShader != null)
            {
                if (value != null && !value.IsNull)
                {
                    effectShader.ParticlePaletteTexture = new AssetLink<SkyrimTextureAssetType>(EffectShaderTexturePathHelper.Normalize(value));
                }
                else
                {
                    effectShader.ParticlePaletteTexture = null;
                }
            }
        }

        public override AssetLinkGetter<SkyrimTextureAssetType>? GetValue(IMajorRecordGetter record)
        {
            var effectShader = TryCastRecord<IEffectShaderGetter>(record, PropertyName);
            if (effectShader != null)
            {
                return effectShader.ParticlePaletteTexture;
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

