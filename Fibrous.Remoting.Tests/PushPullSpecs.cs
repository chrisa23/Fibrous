namespace Fibrous.Remoting.Tests
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;
    
    using FluentAssertions;
    using NUnit.Framework;
    using NetMQ;
    using NetMQ.zmq;

    [TestFixture]
    public class WhenMsgIsSent : PushPullSocketPortSpecs
    {
        [Test]
        public void Test()
        {
            Channel.Subscribe(ClientFiber,
                s =>
                {
                    Received = s;
                    RcvdSignal.Set();
                });
            Push.Publish("test");
            Thread.Sleep(100);
            RcvdSignal.WaitOne(TimeSpan.FromSeconds(1));
            Cleanup();
            Received.Should().BeEquivalentTo("test");
        }
    }

    [TestFixture]
    public class ShouldSendALotFast : PushPullSocketPortSpecs
    {
        [Test]
        public void Test()
        {
            Channel.Subscribe(ClientFiber,
                s =>
                {
                    Received = s;
                    if (s == "test999999")
                        RcvdSignal.Set();
                });
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < 1000000; i++)
                Push.Publish("test" + i);
            RcvdSignal.WaitOne(TimeSpan.FromSeconds(5));
            sw.Stop();
            Cleanup();
            Console.WriteLine("Elapsed: " + sw.ElapsedMilliseconds);
            Received.Should().BeEquivalentTo("test999999");
        }
    }

    public abstract class PushPullSocketPortSpecs
    {
        protected static NetMQContext Context1;
        protected static NetMQContext Context2;
        protected static SendSocket<string> Push;
        protected static PullSocket<string> Pull;
        protected static Fiber ClientFiber;
        protected static string Received;
        protected static ManualResetEvent RcvdSignal;
        protected static IChannel<string> Channel = new Channel<string>();

        public PushPullSocketPortSpecs()
        {
            Context1 = NetMQContext.Create();
            Context2 = NetMQContext.Create();
            RcvdSignal = new ManualResetEvent(false);
            ClientFiber = PoolFiber.StartNew();
            Pull = new PullSocket<string>(Context1, "tcp://*:6002", x => Encoding.Unicode.GetString(x), Channel);
            Push = new SendSocket<string>(Context2,
                "tcp://127.0.0.1:6002",
                s => Encoding.Unicode.GetBytes(s),
                ZmqSocketType.Push,
                false);
        }

        protected void Cleanup()
        {
            ClientFiber.Enqueue(() =>
            {
                Console.WriteLine("Dispose push");
                Push.Dispose();
                Context2.Dispose();
                Thread.Sleep(10);
                Console.WriteLine("Dispose pull");
                Pull.Dispose();
                Context1.Dispose();
                Thread.Sleep(10);
            });
            Thread.Sleep(200);
            ClientFiber.Dispose();
        }
    }
}