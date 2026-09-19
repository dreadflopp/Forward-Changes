using Mutagen.Bethesda;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ForwardChanges.App.Models;

public sealed class StandaloneSettings : INotifyPropertyChanged
{
    private GameRelease _gameRelease = GameRelease.SkyrimSE;
    private string _gameFolderPath = string.Empty;
    private string _dataFolderPath = string.Empty;
    private string _loadOrderFilePath = string.Empty;
    private int _historicalLogsToKeep = 10;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int SettingsVersion { get; set; } = 3;

    public GameRelease GameRelease
    {
        get => _gameRelease;
        set => SetProperty(ref _gameRelease, value);
    }

    public string GameFolderPath
    {
        get => _gameFolderPath;
        set => SetProperty(ref _gameFolderPath, value ?? string.Empty);
    }

    public string DataFolderPath
    {
        get => _dataFolderPath;
        set => SetProperty(ref _dataFolderPath, value ?? string.Empty);
    }

    public string LoadOrderFilePath
    {
        get => _loadOrderFilePath;
        set => SetProperty(ref _loadOrderFilePath, value ?? string.Empty);
    }

    public int HistoricalLogsToKeep
    {
        get => _historicalLogsToKeep;
        set => SetProperty(ref _historicalLogsToKeep, value);
    }

    public PatcherConfiguration Patcher { get; set; } = new();

    public void Normalize()
    {
        SettingsVersion = 3;
        if (GameRelease is not (GameRelease.SkyrimSE or GameRelease.SkyrimSEGog or GameRelease.SkyrimVR))
        {
            GameRelease = GameRelease.SkyrimSE;
        }

        GameFolderPath = GameFolderPath?.Trim() ?? string.Empty;
        DataFolderPath = DataFolderPath?.Trim() ?? string.Empty;
        LoadOrderFilePath = LoadOrderFilePath?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(GameFolderPath) && !string.IsNullOrWhiteSpace(DataFolderPath))
        {
            GameFolderPath = Directory.GetParent(DataFolderPath)?.FullName ?? string.Empty;
        }
        HistoricalLogsToKeep = Math.Clamp(HistoricalLogsToKeep, 0, 100);
        Patcher ??= new PatcherConfiguration();
        Patcher.Normalize();
    }

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
