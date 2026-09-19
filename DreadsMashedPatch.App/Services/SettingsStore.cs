using System.Text.Json;
using System.Text.Json.Serialization;
using DreadsMashedPatch.App.Models;

namespace DreadsMashedPatch.App.Services;

public sealed class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static string SettingsDirectory { get; } = AppContext.BaseDirectory;

    public static string SettingsPath => Path.Combine(SettingsDirectory, "settings.json");

    public async Task<StandaloneSettings> LoadAsync()
    {
        if (!File.Exists(SettingsPath))
        {
            return new StandaloneSettings();
        }

        await using var stream = File.OpenRead(SettingsPath);
        var settings = await JsonSerializer.DeserializeAsync<StandaloneSettings>(stream, JsonOptions)
            ?? new StandaloneSettings();
        settings.Normalize();
        return settings;
    }

    public async Task SaveAsync(StandaloneSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        settings.Normalize();
        var temporaryPath = SettingsPath + ".tmp";
        await using (var stream = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(stream, settings, JsonOptions);
        }

        File.Move(temporaryPath, SettingsPath, overwrite: true);
    }
}
