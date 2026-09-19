using System.Text;
using System.IO;

namespace DreadsMashedPatch.App.Services;

internal sealed class UiTextWriter : TextWriter
{
    private readonly Action<string> _write;
    private readonly StringBuilder _buffer = new();
    private readonly Timer _flushTimer;
    private bool _disposed;

    public UiTextWriter(Action<string> write)
    {
        _write = write;
        _flushTimer = new Timer(_ => FlushBuffer(), null, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100));
    }

    public override Encoding Encoding => Encoding.UTF8;

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

    public override void Flush() => FlushBuffer();

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            _disposed = true;
            _flushTimer.Dispose();
            FlushBuffer();
        }

        base.Dispose(disposing);
    }

    private void FlushBuffer()
    {
        string pending;
        lock (_buffer)
        {
            if (_buffer.Length == 0)
            {
                return;
            }

            pending = _buffer.ToString();
            _buffer.Clear();
        }

        _write(pending);
    }
}
