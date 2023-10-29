using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests;

[TestFixture]
public class ContentionTests
{
    private const int OperationsPerInvoke = 1000000;
    private int i;
    private readonly AutoResetEvent _wait = new(false);

    [Test]
    public void Test()
    {
        using AsyncFiber afiber = new();
        using Fiber fiber = new();
        _count = 2;
        Run(fiber);
        Run(afiber);
        Console.WriteLine("2");

        _count = 4;
        Run(afiber);
        Run(fiber);
        Console.WriteLine("4");

        _count = 10;
        Run(afiber);
        Run(fiber);
        Console.WriteLine("10");
    }

    private void Handler(object obj)
    {
        i++;
        if (i == OperationsPerInvoke)
        {
            _wait.Set();
        }
    }

    private Task AsyncHandler(object obj)
    {
        i++;
        if (i == OperationsPerInvoke)
        {
            _wait.Set();
        }

        return Task.CompletedTask;
    }

    private readonly IChannel<object> _channel = new Channel<object>();
    private int _count;

    public void Run(IFiber fiber)
    {
        using IDisposable sub = _channel.Subscribe(fiber, Handler);

        i = 0;
        for (int j = 0; j < _count; j++)
        {
            _ = Task.Run(Iterate);
        }

        WaitHandle.WaitAny(new WaitHandle[] {_wait});
    }

    private void Iterate()
    {
        int count = OperationsPerInvoke / _count;
        for (int j = 0; j < count; j++)
        {
            _channel.Publish(null);
        }
    }

    public void Run(IAsyncFiber fiber)
    {
        using IDisposable sub = _channel.Subscribe(fiber, AsyncHandler);

        i = 0;
        for (int j = 0; j < _count; j++)
        {
            _ = Task.Run(Iterate);
        }

        WaitHandle.WaitAny(new WaitHandle[] {_wait});
    }
}
