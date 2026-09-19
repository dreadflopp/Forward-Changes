using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim.Assets;

namespace DreadsMashedPatch.PropertyHandlers.General;

internal static class ModelPathHelper
{
    public static string NormalizeAsset(IAssetLinkGetter<SkyrimModelAssetType> file)
        => file.GivenPath;

    public static string NormalizePath(string path)
        => AssetPathHelper.NormalizeForComparison(path);
}
