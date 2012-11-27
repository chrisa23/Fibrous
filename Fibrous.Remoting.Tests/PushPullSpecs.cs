namespace Fibrous.Remoting.Tests
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;
    using CrossroadsIO;
    using Fibrous.Fibers;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class WhenMsgIsSent : PushPullSocketPortSpecs
    {
        [Test]
        public void Test()
        {
            Pull.Subscribe(ClientFiber,
                s =>
                {
                    Received = s;
                    RcvdSignal.Set();
                });
            Push.Publish("test");
            RcvdSignal.WaitOne(TimeSpan.FromSeconds(1));
            Received.Should().BeEquivalentTo("test");
        }
    }

    [TestFixture]
    public class ShouldSendALotFast : PushPullSocketPortSpecs
    {
        [Test]
        public void Test()
        {
            Pull.Subscribe(ClientFiber,
                s =>
                {
                    Received = s;
                    if (s == "test999999")
                        RcvdSignal.Set();
                });
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < 1000000; i++)
                Push.Publish("test" + i);
            RcvdSignal.WaitOne(TimeSpan.FromSeconds(10));
            sw.Stop();
            Console.WriteLine("Elapsed: " + sw.ElapsedMilliseconds);
            Received.Should().BeEquivalentTo("test999999");
            Cleanup();
        }
    }

    public abstract class PushPullSocketPortSpecs
    {
        protected static Context Context1;
        protected static Context Context2;
        protected static SendSocket<string> Push;
        protected static PullSocketPort<string> Pull;
        protected static IFiber ClientFiber;
        protected static string Received;
        protected static ManualResetEvent RcvdSignal;

        public PushPullSocketPortSpecs()
        {
            Context1 = Context.Create();
            Context2 = Context.Create();
            RcvdSignal = new ManualResetEvent(false);
            ClientFiber = new StubFiber();
            Pull = new PullSocketPort<string>(Context1, "tcp://*:6002", x => Encoding.Unicode.GetString(x));
            Push = new SendSocket<string>(Context2,
                "tcp://localhost:6002",
                s => Encoding.Unicode.GetBytes(s),
                SocketType.PUSH,
                false);
        }

        protected void Cleanup()
        {
            RcvdSignal.Dispose();
            ClientFiber.Dispose();
            Push.Dispose();
            Console.WriteLine("Dispose pull");
            Pull.Dispose();
            Console.WriteLine("Dispose push");
            Context1.Dispose();
            Context2.Dispose();
            Thread.Sleep(100);
        }
    }
}