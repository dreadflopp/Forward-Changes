using System.Text.Json;

namespace DreadsMashedPatch.App.Services;

public sealed class MasterRuleStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public async Task<IReadOnlyList<VirtualMasterRule>> LoadAsync(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        await using var stream = File.OpenRead(path);
        var importedRules = await JsonSerializer.DeserializeAsync<List<VirtualMasterRule?>>(stream, JsonOptions)
            ?? throw new InvalidDataException("The selected file does not contain a master-rule list.");

        if (importedRules.Any(rule => rule is null))
        {
            throw new InvalidDataException("The selected file contains an empty master-rule entry.");
        }

        return importedRules
            .Select(rule => rule!.Normalize())
            .ToList();
    }

    public async Task SaveAsync(string path, IEnumerable<VirtualMasterRule> rules)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(rules);

        var exportedRules = rules
            .Select(rule => rule.Copy().Normalize())
            .ToList();
        var temporaryPath = path + "." + Guid.NewGuid().ToString("N") + ".tmp";

        try
        {
            await using (var stream = File.Create(temporaryPath))
            {
                await JsonSerializer.SerializeAsync(stream, exportedRules, JsonOptions);
            }

            File.Move(temporaryPath, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }
}
