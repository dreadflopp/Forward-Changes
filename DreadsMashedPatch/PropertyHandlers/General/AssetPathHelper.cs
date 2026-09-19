using Mutagen.Bethesda.Assets;
using Mutagen.Bethesda.Plugins.Assets;

namespace DreadsMashedPatch.PropertyHandlers.General;

internal static class AssetPathHelper
{
    public static AssetLink<TAssetType>? Copy<TAssetType>(IAssetLinkGetter<TAssetType>? source)
        where TAssetType : class, IAssetType
        => source == null ? null : new AssetLink<TAssetType>(source.GivenPath);

    public static bool AreEqual<TAssetType>(
        IAssetLinkGetter<TAssetType>? left,
        IAssetLinkGetter<TAssetType>? right)
        where TAssetType : class, IAssetType
    {
        if (left == null || right == null)
        {
            return left == null && right == null;
        }

        return string.Equals(
            NormalizeForComparison(left.GivenPath),
            NormalizeForComparison(right.GivenPath),
            StringComparison.OrdinalIgnoreCase);
    }

    public static string NormalizeForComparison(string path) => path.Replace('/', '\\');

    public static string Format<TAssetType>(IAssetLinkGetter<TAssetType>? assetLink)
        where TAssetType : class, IAssetType
    {
        if (assetLink == null)
        {
            return "null";
        }

        var serialized = assetLink.GivenPath;
        var lookup = assetLink.DataRelativePath.ToString();
        return string.Equals(serialized, lookup, StringComparison.OrdinalIgnoreCase)
            ? serialized
            : $"{serialized} (lookup: {lookup})";
    }
}
