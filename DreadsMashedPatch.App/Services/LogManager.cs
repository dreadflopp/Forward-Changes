using System.Text;

namespace ForwardChanges.App.Services;

public static class LogManager
{
    private const string CurrentLogName = "ForwardChanges-current.log";
    private const string HistoryPattern = "ForwardChanges-*.log";

    public static string LogsDirectory { get; } = Path.Combine(AppContext.BaseDirectory, "Logs");

    public static LogSession StartRun(int historicalLogsToKeep)
    {
        historicalLogsToKeep = Math.Clamp(historicalLogsToKeep, 0, 100);
        Directory.CreateDirectory(LogsDirectory);

        var currentPath = Path.Combine(LogsDirectory, CurrentLogName);
        ArchiveCurrentLog(currentPath);
        PruneHistory(historicalLogsToKeep);
        return new LogSession(currentPath);
    }

    private static void ArchiveCurrentLog(string currentPath)
    {
        if (!File.Exists(currentPath))
        {
            return;
        }

        if (new FileInfo(currentPath).Length == 0)
        {
            File.Delete(currentPath);
            return;
        }

        var timestamp = File.GetLastWriteTime(currentPath).ToString("yyyy-MM-dd_HH-mm-ss-fff");
        var historyPath = Path.Combine(LogsDirectory, $"ForwardChanges-{timestamp}.log");
        File.Move(currentPath, GetAvailableHistoryPath(historyPath));
    }

    private static string GetAvailableHistoryPath(string desiredPath)
    {
        if (!File.Exists(desiredPath))
        {
            return desiredPath;
        }

        var directory = Path.GetDirectoryName(desiredPath)!;
        var name = Path.GetFileNameWithoutExtension(desiredPath);
        for (var suffix = 2; ; suffix++)
        {
            var candidate = Path.Combine(directory, $"{name}-{suffix}.log");
            if (!File.Exists(candidate))
            {
                return candidate;
            }
        }
    }

    private static void PruneHistory(int logsToKeep)
    {
        foreach (var staleLog in Directory
            .EnumerateFiles(LogsDirectory, HistoryPattern)
            .Where(path => !string.Equals(
                Path.GetFileName(path),
                CurrentLogName,
                StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
            .Skip(logsToKeep))
        {
            File.Delete(staleLog);
        }
    }
}

public sealed class LogSession : IDisposable
{
    private readonly StreamWriter _writer;
    private readonly object _sync = new();

    internal LogSession(string path)
    {
        _writer = new StreamWriter(
            new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
        {
            AutoFlush = true
        };
    }

    public void Write(string text)
    {
        lock (_sync)
        {
            _writer.Write(text);
        }
    }

    public void Dispose()
    {
        lock (_sync)
        {
            _writer.Dispose();
        }
    }
}
