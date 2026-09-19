using Mutagen.Bethesda;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Installs.DI;
using Mutagen.Bethesda.Plugins.Order;

namespace DreadsMashedPatch.App.Services;

public static class PathDiscovery
{
    public sealed record Installation(
        GameRelease Release,
        string DisplayName,
        string GameFolder,
        string DataFolder,
        string? LoadOrderFile);

    private static readonly GameRelease[] SupportedReleases =
    [
        GameRelease.SkyrimSE,
        GameRelease.SkyrimSEGog,
        GameRelease.SkyrimVR
    ];

    public static string? TryFindDataFolder(GameRelease release)
    {
        IDataDirectoryLookup lookup = new GameLocatorLookupCache();
        return lookup.TryGet(release, out var path) ? path.ToString() : null;
    }

    public static string? TryFindLoadOrder(GameRelease release)
    {
        return PluginListings.TryGetListingsFile(release, out var path)
            ? path.ToString()
            : null;
    }

    public static IReadOnlyList<Installation> DiscoverInstallations()
    {
        var installations = new List<Installation>();
        foreach (var release in SupportedReleases)
        {
            var dataFolder = TryFindDataFolder(release);
            var gameFolder = dataFolder is null ? null : Directory.GetParent(dataFolder)?.FullName;
            if (dataFolder is not null && gameFolder is not null)
            {
                installations.Add(new Installation(
                    release,
                    GetDisplayName(release),
                    gameFolder,
                    dataFolder,
                    TryFindLoadOrder(release)));
            }
        }

        return installations
            .DistinctBy(x => NormalizePath(x.GameFolder), StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string GetDisplayName(GameRelease release) => release switch
    {
        GameRelease.SkyrimSEGog => "Skyrim SE/AE — GOG",
        GameRelease.SkyrimVR => "Skyrim VR — Steam",
        _ => "Skyrim SE/AE — Steam"
    };

    private static string NormalizePath(string path) =>
        Path.TrimEndingDirectorySeparator(Path.GetFullPath(path.Trim()));
}
