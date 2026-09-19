using System.Text;
using ForwardChanges.App.Models;
using Mutagen.Bethesda.Plugins;

namespace ForwardChanges.App.Services;

public sealed record PreparedLoadOrder(
    string Path,
    string CreationClubPath,
    int CreationClubPluginCount,
    IReadOnlySet<ModKey> CreationClubPlugins,
    bool OutputPluginFound,
    int ListingsAfterOutput);

public static class LoadOrderPreparer
{
    private const string OutputPluginName = "ForwardChanges.esp";

    public static async Task<PreparedLoadOrder> CreateAsync(StandaloneSettings settings)
    {
        var creationClubPath = System.IO.Path.Combine(settings.GameFolderPath, "Skyrim.ccc");
        var mergedLines = new List<string>();
        var creationClubPlugins = new HashSet<ModKey>();
        var creationClubCount = 0;

        if (File.Exists(creationClubPath))
        {
            foreach (var rawLine in await File.ReadAllLinesAsync(creationClubPath))
            {
                var pluginName = rawLine.Trim();
                if (pluginName.Length == 0)
                {
                    continue;
                }

                var modKey = ModKey.FromNameAndExtension(pluginName.AsSpan());
                if (!File.Exists(System.IO.Path.Combine(settings.DataFolderPath, modKey.FileName.String)))
                {
                    continue;
                }

                if (!creationClubPlugins.Add(modKey))
                {
                    continue;
                }

                mergedLines.Add($"*{modKey.FileName.String}");
                creationClubCount++;
            }
        }

        mergedLines.AddRange(await File.ReadAllLinesAsync(settings.LoadOrderFilePath));

        var outputIndex = mergedLines.FindIndex(line =>
            string.Equals(GetPluginName(line), OutputPluginName, StringComparison.OrdinalIgnoreCase));
        var listingsAfterOutput = outputIndex < 0
            ? 0
            : mergedLines.Skip(outputIndex + 1).Count(line => GetPluginName(line) is not null);

        var preparedPath = System.IO.Path.Combine(
            SettingsStore.SettingsDirectory,
            $".forwardchanges-loadorder-{Guid.NewGuid():N}.txt");
        await File.WriteAllLinesAsync(preparedPath, mergedLines, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        return new PreparedLoadOrder(
            preparedPath,
            creationClubPath,
            creationClubCount,
            creationClubPlugins,
            outputIndex >= 0,
            listingsAfterOutput);
    }

    private static string? GetPluginName(string line)
    {
        var commentIndex = line.IndexOf('#');
        if (commentIndex >= 0)
        {
            line = line[..commentIndex];
        }

        var value = line.Trim();
        if (value.StartsWith('*'))
        {
            value = value[1..].TrimStart();
        }

        if (value.Length == 0)
        {
            return null;
        }

        return ModKey.TryFromFileName(value, out var modKey)
            ? modKey.FileName.String
            : null;
    }
}
