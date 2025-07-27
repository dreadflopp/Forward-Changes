using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim.Assets;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.EffectShader
{
    public class MembranePaletteTextureHandler : AbstractPropertyHandler<AssetLinkGetter<SkyrimTextureAssetType>>
    {
        public override string PropertyName => "MembranePaletteTexture";

        public override void SetValue(IMajorRecord record, AssetLinkGetter<SkyrimTextureAssetType>? value)
        {
            if (record is IEffectShader effectShader)
            {
                if (value != null && !value.IsNull)
                {
                    effectShader.MembranePaletteTexture = new AssetLink<SkyrimTextureAssetType>(value.ToString());
                }
                else
                {
                    effectShader.MembranePaletteTexture = null;
                }
            }
            else
            {
                Console.WriteLine($"Error: Record is not an EffectShader for {PropertyName}");
            }
        }

        public override AssetLinkGetter<SkyrimTextureAssetType>? GetValue(IMajorRecordGetter record)
        {
            if (record is IEffectShaderGetter effectShader)
            {
                return effectShader.MembranePaletteTexture;
            }
            else
            {
                Console.WriteLine($"Error: Record is not an EffectShader for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(AssetLinkGetter<SkyrimTextureAssetType>? value1, AssetLinkGetter<SkyrimTextureAssetType>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return string.Equals(value1.ToString(), value2.ToString(), System.StringComparison.OrdinalIgnoreCase);
        }
    }
}

