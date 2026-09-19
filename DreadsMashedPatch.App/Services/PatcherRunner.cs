using DreadsMashedPatch.App.Models;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Analysis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Synthesis.CLI;

namespace DreadsMashedPatch.App.Services;

public sealed class PatcherRunner
{
    private const string OutputPluginName = "Dread's Mashed Patch.esp";

    public static string GetOutputPath(StandaloneSettings settings) =>
        Path.Combine(settings.DataFolderPath, OutputPluginName);

    public async Task RunAsync(StandaloneSettings settings, Action<string> writeLog)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(writeLog);

        settings.Normalize();

        var outputPath = GetOutputPath(settings);
        var outputModKey = ModKey.FromNameAndExtension(OutputPluginName.AsSpan());
        var preparedLoadOrder = await LoadOrderPreparer.CreateAsync(settings);
        PatcherSettings.Apply(settings.Patcher, preparedLoadOrder.CreationClubPlugins);

        var removedSplitOutputs = CleanupSplitOutputs(outputPath);
        if (removedSplitOutputs > 0)
        {
            writeLog($"Removed {removedSplitOutputs} output file(s) from the previous split patch.{Environment.NewLine}");
        }

        writeLog($"Game release: {settings.GameRelease}{Environment.NewLine}");
        writeLog($"Game folder: {settings.GameFolderPath}{Environment.NewLine}");
        writeLog($"Data folder: {settings.DataFolderPath}{Environment.NewLine}");
        writeLog($"Load order: {settings.LoadOrderFilePath}{Environment.NewLine}");
        if (File.Exists(preparedLoadOrder.CreationClubPath))
        {
            writeLog($"Creation Club list: {preparedLoadOrder.CreationClubPath} "
                + $"({preparedLoadOrder.CreationClubPluginCount} installed plugins){Environment.NewLine}");
        }
        else
        {
            writeLog($"Creation Club list not found at {preparedLoadOrder.CreationClubPath}; "
                + $"continuing with plugins.txt entries.{Environment.NewLine}");
        }

        writeLog(preparedLoadOrder.OutputPluginFound
            ? $"{OutputPluginName} was found in the load order; "
                + $"{preparedLoadOrder.ListingsAfterOutput} later listings will not be read.{Environment.NewLine}"
            : $"{OutputPluginName} was not found in the load order; the complete enabled load order will be read.{Environment.NewLine}");

        var arguments = new RunSynthesisMutagenPatcher
        {
            OutputPath = outputPath,
            GameRelease = settings.GameRelease,
            DataFolderPath = settings.DataFolderPath,
            LoadOrderFilePath = preparedLoadOrder.Path,
            ExtraDataFolder = SettingsStore.SettingsDirectory,
            PersistencePath = Path.Combine(SettingsStore.SettingsDirectory, "Persistence"),
            PatcherName = "Dread's Mashed Patch",
            // Synthesis uses this primary ModKey as the load-order cutoff, removing
            // it and every later listing before importing any input plugins. Keep
            // this as the unsuffixed key even when automatic output splitting is on.
            ModKey = outputModKey.FileName.String,
            // The temporary load order already contains the Creation Club entries
            // from the explicitly selected game folder.
            LoadOrderIncludesCreationClub = true,
            SplitIfMaxMastersExceeded = true
        };

        try
        {
            await Task.Run(async () =>
            {
                var originalOut = Console.Out;
                var originalError = Console.Error;
                using var writer = new UiTextWriter(writeLog);
                Console.SetOut(writer);
                Console.SetError(writer);

                try
                {
                    var pipeline = SynthesisPipeline.Instance
                        .AddPatch<ISkyrimMod, ISkyrimModGetter>(Program.RunPatch);
                    await pipeline.Run(arguments);
                }
                finally
                {
                    Console.SetOut(originalOut);
                    Console.SetError(originalError);
                }
            });
        }
        finally
        {
            File.Delete(preparedLoadOrder.Path);
        }
    }

    private static int CleanupSplitOutputs(string outputPath)
    {
        var outputDirectory = Path.GetDirectoryName(outputPath);
        if (string.IsNullOrEmpty(outputDirectory) || !Directory.Exists(outputDirectory))
        {
            return 0;
        }

        var baseName = Path.GetFileNameWithoutExtension(outputPath);
        var extension = Path.GetExtension(outputPath);
        var removed = 0;

        foreach (var candidate in Directory.EnumerateFiles(outputDirectory, $"{baseName}_*{extension}"))
        {
            var candidateName = Path.GetFileNameWithoutExtension(candidate);
            // Match the exact convention used by Synthesis auto-splitting. The
            // unsuffixed primary output is deliberately never touched: it remains
            // the stable ModKey that marks the load-order cutoff on every rerun.
            if (!MultiModFileAnalysis.IsSplitFileName(candidateName, baseName, out var splitIndex)
                || splitIndex < 2)
            {
                continue;
            }

            File.Delete(candidate);
            removed++;
        }

        return removed;
    }
}
