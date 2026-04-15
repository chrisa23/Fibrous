using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests;

[TestFixture]
public class DisposalRegressionTests
{
    [Test]
    public void Disposables_AddAfterDispose_DisposesImmediately()
    {
        using TrackingDisposable item = new();
        Disposables disposables = new();

        disposables.Dispose();
        disposables.Add(item);

        Assert.That(item.Disposed, Is.True);
    }

    [Test]
    public void SchedulingAfterFiberDispose_DoesNotExecute()
    {
        using AutoResetEvent reset = new(false);
        Fiber fiber = new();
        fiber.Dispose();

        IDisposable sub = fiber.Schedule(() => reset.Set(), TimeSpan.FromMilliseconds(50));

        Assert.That(reset.WaitOne(TimeSpan.FromMilliseconds(200)), Is.False);
        sub.Dispose();
    }

    private sealed class TrackingDisposable : IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose() => Disposed = true;
    }
}
