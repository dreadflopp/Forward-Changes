using System;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim.Assets;

namespace ForwardChanges.PropertyHandlers.General
{
    internal static class TexturePathHelper
    {
        private const string TexturesPrefix = "Textures\\";

        public static string Normalize(AssetLinkGetter<SkyrimTextureAssetType> assetLink)
        {
            var givenPath = assetLink.GivenPath;
            if (!string.IsNullOrWhiteSpace(givenPath))
            {
                return givenPath;
            }

            return Normalize(assetLink.DataRelativePath.ToString());
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