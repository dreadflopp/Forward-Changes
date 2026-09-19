using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim.Assets;

namespace ForwardChanges.PropertyHandlers.General
{
    internal static class TexturePathHelper
    {
        public static string Normalize(AssetLinkGetter<SkyrimTextureAssetType> assetLink)
            => assetLink.GivenPath;

        public static bool AreEqual(
            IAssetLinkGetter<SkyrimTextureAssetType>? left,
            IAssetLinkGetter<SkyrimTextureAssetType>? right)
            => AssetPathHelper.AreEqual(left, right);
    }
}
