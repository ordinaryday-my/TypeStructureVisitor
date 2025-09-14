using System.Collections.Immutable;
using System.Text;

namespace TypeStructureVisitor;

public class MultiTextWriter : TextWriter
{
    private readonly IReadOnlyCollection<TextWriter> _targetWriters;
    private bool _isDisposed;

    /// <summary>
    /// 初始化MultipleForwardingTextWriter实例
    /// </summary>
    /// <param name="targetWriters">要转发到的目标TextWriter集合</param>
    /// <exception cref="ArgumentNullException">当目标集合为null时抛出</exception>
    /// <exception cref="ArgumentException">当目标集合为空时抛出</exception>
    public MultiTextWriter(IEnumerable<TextWriter> targetWriters)
    {
        _targetWriters = new List<TextWriter>(targetWriters ?? throw new ArgumentNullException(nameof(targetWriters)))
            .Distinct().ToImmutableList();

        if (_targetWriters.Count == 0)
        {
            throw new ArgumentException("至少需要指定一个目标TextWriter", nameof(targetWriters));
        }
    }

    /// <summary>
    /// 获取第一个目标写入器的编码格式
    /// </summary>
    public override Encoding Encoding => _targetWriters.First().Encoding;

    /// <summary>
    /// 将字符写入所有目标流
    /// </summary>
    public override void Write(char value)
    {
        CheckDisposed();
        Parallel.ForEach(_targetWriters, writer => writer.Write(value));
    }

    /// <summary>
    /// 将字符数组写入所有目标流
    /// </summary>
    public override void Write(char[] buffer, int index, int count)
    {
        CheckDisposed();
        Parallel.ForEach(_targetWriters, writer => writer.Write(buffer));
    }

    /// <summary>
    /// 将字符串写入所有目标流
    /// </summary>
    public override void Write(string? value)
    {
        CheckDisposed();
        Parallel.ForEach(_targetWriters, writer => writer.Write(value));
    }

    /// <summary>
    /// 写入换行符到所有目标流
    /// </summary>
    public override void WriteLine()
    {
        CheckDisposed();
        Parallel.ForEach(_targetWriters, writer => writer.WriteLine());
    }

    /// <summary>
    /// 将带换行符的字符串写入所有目标流
    /// </summary>
    public override void WriteLine(string? value)
    {
        CheckDisposed();
        Parallel.ForEach(_targetWriters, writer => writer.WriteLine(value));
    }

    /// <summary>
    /// 刷新所有目标流的缓冲区
    /// </summary>
    public override void Flush()
    {
        CheckDisposed();
        Parallel.ForEach(_targetWriters, writer => writer.Flush());
    }

    /// <summary>
    /// 释放所有目标流资源
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                // 释放所有托管资源
                foreach (var writer in _targetWriters)
                {
                    if (writer != Console.Out && writer != Console.Error)
                    {
                        writer.Dispose();
                    }
                }
            }

            _isDisposed = true;
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// 检查对象是否已释放
    /// </summary>
    /// <exception cref="ObjectDisposedException">当对象已释放时抛出</exception>
    private void CheckDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(MultiTextWriter));
        }
    }
}