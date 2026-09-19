using System;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim.Assets;

namespace DreadsMashedPatch.PropertyHandlers.EffectShader
{
    internal static class EffectShaderTexturePathHelper
    {
        private const string TexturesPrefix = "Textures\\";

        public static string Normalize(AssetLinkGetter<SkyrimTextureAssetType> assetLink)
        {
            return DreadsMashedPatch.PropertyHandlers.General.TexturePathHelper.Normalize(assetLink);
        }

        public static string Normalize(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            if (path.StartsWith(TexturesPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return path.Substring(TexturesPrefix.Length);
            }

            return path;
        }
    }
}