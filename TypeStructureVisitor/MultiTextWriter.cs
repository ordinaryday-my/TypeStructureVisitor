using System.Collections.Immutable;
using System.Text;

namespace TypeStructureVisitor;

public class MultiTextWriter : TextWriter
{
    private readonly IReadOnlyCollection<TextWriter> _targetWriters;
    private bool _isDisposed;

    public MultiTextWriter(IEnumerable<TextWriter> targetWriters)
    {
        _targetWriters = new List<TextWriter>(targetWriters ?? throw new ArgumentNullException(nameof(targetWriters)))
            .Distinct().ToImmutableList();

        if (_targetWriters.Count == 0)
        {
            throw new ArgumentException("至少需要指定一个目标TextWriter", nameof(targetWriters));
        }
    }

    public override Encoding Encoding 
    { 
        get
        {
            CheckDisposed();
            // 检查所有写入器是否有相同的编码
            var encodings = _targetWriters.Select(w => w.Encoding).Distinct().ToList();
            if (encodings.Count > 1)
            {
                throw new InvalidOperationException("所有目标写入器必须使用相同的编码");
            }
            return encodings.First();
        }
    }

    public override void Write(char value)
    {
        CheckDisposed();
        foreach (var writer in _targetWriters)
        {
            writer.Write(value);
        }
    }

    public override void Write(char[] buffer, int index, int count)
    {
        CheckDisposed();
        foreach (var writer in _targetWriters)
        {
            writer.Write(buffer, index, count); // 修正参数传递
        }
    }

    public override void Write(string? value)
    {
        CheckDisposed();
        foreach (var writer in _targetWriters)
        {
            writer.Write(value);
        }
    }

    public override void WriteLine()
    {
        CheckDisposed();
        foreach (var writer in _targetWriters)
        {
            writer.WriteLine();
        }
    }

    public override void WriteLine(string? value)
    {
        CheckDisposed();
        foreach (var writer in _targetWriters)
        {
            writer.WriteLine(value);
        }
    }

    public override void Flush()
    {
        CheckDisposed();
        foreach (var writer in _targetWriters)
        {
            writer.Flush();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                // 只释放由当前实例创建的写入器，或明确指定需要由当前实例管理的写入器
                foreach (var writer in _targetWriters)
                {
                    // 可以考虑引入一个标志来确定是否需要释放传入的写入器
                    writer.Dispose();
                }
            }

            _isDisposed = true;
        }

        base.Dispose(disposing);
    }

    private void CheckDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(MultiTextWriter));
        }
    }
}