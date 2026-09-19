using DreadsMashedPatch.App.Models;
using Mutagen.Bethesda.Plugins;
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
            ModKey = outputModKey.FileName.String,
            // The temporary load order already contains the Creation Club entries
            // from the explicitly selected game folder.
            LoadOrderIncludesCreationClub = true
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
}
