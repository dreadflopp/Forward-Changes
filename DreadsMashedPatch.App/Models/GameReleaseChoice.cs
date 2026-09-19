using Mutagen.Bethesda;

namespace DreadsMashedPatch.App.Models;

public sealed record GameReleaseChoice(GameRelease Value, string DisplayName)
{
    public static IReadOnlyList<GameReleaseChoice> Supported { get; } =
    [
        new(GameRelease.SkyrimSE, "Skyrim Special Edition / Anniversary Edition — Steam"),
        new(GameRelease.SkyrimSEGog, "Skyrim Special Edition / Anniversary Edition — GOG"),
        new(GameRelease.SkyrimVR, "Skyrim VR")
    ];
}
