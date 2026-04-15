using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using NUnit.Framework;

namespace Fibrous.WPF.Tests;

[TestFixture]
public class DispatcherFiberTests
{
    [Test]
    public void Enqueue_RunsOnDispatcherThread_InOrder()
    {
        using DispatcherHost host = DispatcherHost.Start();
        using DispatcherFiber fiber = new(dispatcher: host.Dispatcher);
        using AutoResetEvent reset = new(false);

        int first = 0;
        int second = 0;
        int callbackThread = 0;

        fiber.Enqueue(() =>
        {
            callbackThread = Environment.CurrentManagedThreadId;
            first = 1;
        });

        fiber.Enqueue(() =>
        {
            second = first + 1;
            reset.Set();
        });

        Assert.That(reset.WaitOne(TimeSpan.FromSeconds(2)), Is.True);
        Assert.That(callbackThread, Is.EqualTo(host.ThreadId));
        Assert.That(second, Is.EqualTo(2));
    }

    [Test]
    public void WpfFiberFactory_UsesProvidedDispatcher()
    {
        using DispatcherHost host = DispatcherHost.Start();
        WpfFiberFactory factory = new(host.Dispatcher);
        using IFiber fiber = factory.CreateFiber(_ => Assert.Fail("No exception expected"));
        using AutoResetEvent reset = new(false);

        int callbackThread = 0;
        fiber.Enqueue(() =>
        {
            callbackThread = Environment.CurrentManagedThreadId;
            reset.Set();
        });

        Assert.That(reset.WaitOne(TimeSpan.FromSeconds(2)), Is.True);
        Assert.That(callbackThread, Is.EqualTo(host.ThreadId));
    }

    private sealed class DispatcherHost : IDisposable
    {
        private readonly AutoResetEvent _started = new(false);
        private readonly Thread _thread;

        private DispatcherHost()
        {
            _thread = new Thread(ThreadStart)
            {
                IsBackground = true,
                Name = "Fibrous.WPF.Tests.DispatcherHost"
            };
            _thread.SetApartmentState(ApartmentState.STA);
        }

        public Dispatcher Dispatcher { get; private set; }

        public int ThreadId { get; private set; }

        public static DispatcherHost Start()
        {
            DispatcherHost host = new();
            host._thread.Start();
            Assert.That(host._started.WaitOne(TimeSpan.FromSeconds(2)), Is.True);
            return host;
        }

        public void Dispose()
        {
            Dispatcher.InvokeShutdown();
            Assert.That(_thread.Join(TimeSpan.FromSeconds(2)), Is.True);
            _started.Dispose();
        }

        private void ThreadStart()
        {
            ThreadId = Environment.CurrentManagedThreadId;
            Dispatcher = Dispatcher.CurrentDispatcher;
            _started.Set();
            Dispatcher.Run();
        }
    }
}
