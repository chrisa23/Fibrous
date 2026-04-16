using System;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Fibrous.Benchmark;

[MemoryDiagnoser]
public class QueueSizeContention
{
    private const int OperationsPerInvoke = 1_000_000;
    private readonly Channel<object> _channel = new();
    private readonly AutoResetEvent _wait = new(false);
    private int _count;

    [Params(1024, 2048, 4096)]
    public int QueueSize { get; set; }

    private Task AsyncHandler(object _)
    {
        if (Interlocked.Increment(ref _count) == OperationsPerInvoke)
        {
            _wait.Set();
        }

        return Task.CompletedTask;
    }

    private void Iterate()
    {
        int count = OperationsPerInvoke / 2;
        for (int i = 0; i < count; i++)
        {
            _channel.Publish(null);
        }
    }

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public void Contention()
    {
        using Fiber fiber = new(size: QueueSize);
        using IDisposable sub = _channel.Subscribe(fiber, AsyncHandler);

        _count = 0;
        Task t1 = Task.Run(Iterate);
        Task t2 = Task.Run(Iterate);

        WaitHandle.WaitAny(new WaitHandle[] { _wait });
        Task.WaitAll(t1, t2);
    }
}
