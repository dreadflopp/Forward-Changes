using System.Text;
using System.IO;

namespace DreadsMashedPatch.App.Services;

internal sealed class UiTextWriter : TextWriter
{
    private static readonly TimeSpan FlushInterval = TimeSpan.FromMilliseconds(500);

    private readonly Action<string> _writeFullLog;
    private readonly Action<string> _writeUiLog;
    private readonly StringBuilder _buffer = new();
    private readonly StringBuilder _incompleteLine = new();
    private readonly object _flushSync = new();
    private readonly Timer _flushTimer;
    private bool _disposed;

    public UiTextWriter(Action<string> writeFullLog, Action<string> writeUiLog)
    {
        _writeFullLog = writeFullLog;
        _writeUiLog = writeUiLog;
        _flushTimer = new Timer(_ => FlushBuffer(flushIncompleteLine: false), null, FlushInterval, FlushInterval);
    }

    public override Encoding Encoding => Encoding.UTF8;

    public int WarningCount { get; private set; }

    public int ErrorCount { get; private set; }

    public override void Write(char value)
    {
        lock (_buffer)
        {
            _buffer.Append(value);
        }
    }

    public override void Write(string? value)
    {
        if (value is not null)
        {
            lock (_buffer)
            {
                _buffer.Append(value);
            }
        }
    }

    public override void WriteLine(string? value)
    {
        lock (_buffer)
        {
            _buffer.AppendLine(value);
        }
    }

    public override void Flush() => FlushBuffer(flushIncompleteLine: false);

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            _disposed = true;
            _flushTimer.Dispose();
            FlushBuffer(flushIncompleteLine: true);
        }

        base.Dispose(disposing);
    }

    private void FlushBuffer(bool flushIncompleteLine)
    {
        lock (_flushSync)
        {
            string pending;
            lock (_buffer)
            {
                pending = _buffer.ToString();
                _buffer.Clear();
            }

            if (pending.Length > 0)
            {
                _writeFullLog(pending);
            }

            var uiOutput = FilterUiOutput(pending, flushIncompleteLine);
            if (uiOutput.Length > 0)
            {
                _writeUiLog(uiOutput);
            }
        }
    }

    private string FilterUiOutput(string pending, bool flushIncompleteLine)
    {
        var output = new StringBuilder();
        foreach (var character in pending)
        {
            if (character == '\n')
            {
                AppendVisibleLine(output);
            }
            else if (character != '\r')
            {
                _incompleteLine.Append(character);
            }
        }

        if (flushIncompleteLine && _incompleteLine.Length > 0)
        {
            AppendVisibleLine(output);
        }

        return output.ToString();
    }

    private void AppendVisibleLine(StringBuilder output)
    {
        var line = _incompleteLine.ToString();
        _incompleteLine.Clear();

        switch (Classify(line))
        {
            case UiLogLineKind.Warning:
                WarningCount++;
                output.AppendLine(line);
                break;
            case UiLogLineKind.Error:
                ErrorCount++;
                output.AppendLine(line);
                break;
            case UiLogLineKind.Progress:
                output.AppendLine(line);
                break;
        }
    }

    private static UiLogLineKind Classify(string line)
    {
        var trimmed = line.TrimStart();

        if (trimmed.Contains("[Error]", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("Error", StringComparison.OrdinalIgnoreCase)
            || trimmed.Contains("] Error ", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("Exception:", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("Unhandled exception", StringComparison.OrdinalIgnoreCase)
            || trimmed.Contains("FAILED after", StringComparison.OrdinalIgnoreCase))
        {
            return UiLogLineKind.Error;
        }

        if (trimmed.Contains("[Warning]", StringComparison.OrdinalIgnoreCase)
            || trimmed.Contains("Warning:", StringComparison.OrdinalIgnoreCase))
        {
            return UiLogLineKind.Warning;
        }

        if (IsProgressLine(trimmed))
        {
            return UiLogLineKind.Progress;
        }

        return UiLogLineKind.Hidden;
    }

    private static bool IsProgressLine(string line)
    {
        return line.Equals("Prepping state.", StringComparison.Ordinal)
            || line.Equals("Running patch.", StringComparison.Ordinal)
            || line.StartsWith("Starting Mashed Patch", StringComparison.Ordinal)
            || (line.StartsWith("Processing ", StringComparison.Ordinal)
                && (line.EndsWith(" records", StringComparison.Ordinal)
                    || line.EndsWith(" record types", StringComparison.Ordinal)))
            || (line.StartsWith("Completed processing ", StringComparison.Ordinal)
                && line.EndsWith(" records", StringComparison.Ordinal))
            || line.StartsWith("Getting all contexts", StringComparison.Ordinal)
            || line.StartsWith("Filtering contexts", StringComparison.Ordinal)
            || line.Equals("Mashed Patch patcher completed.", StringComparison.Ordinal)
            || line.Equals("Finished patch.", StringComparison.Ordinal)
            || line.StartsWith("Writing to output:", StringComparison.Ordinal);
    }

    private enum UiLogLineKind
    {
        Hidden,
        Progress,
        Warning,
        Error
    }
}
