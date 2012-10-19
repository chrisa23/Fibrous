namespace Fibrous.Remoting.Tests.PubSub
{
    using System;
    using System.Text;
    using System.Threading;
    using CrossroadsIO;
    using Fibrous.Fibers;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class WhenMsgIsSentPubSub : PubSubSpecs
    {
        [Test]
        public void Test()
        {
            Subscriber.Subscribe(ClientFiber,
                s =>
                {
                    Received = s;
                    RcvdSignal.Set();
                });
            Thread.Sleep(10);
            Send.Publish("test");
            RcvdSignal.WaitOne(TimeSpan.FromSeconds(1));
            Received.Should().BeEquivalentTo("test");
            Cleanup();
        }
    }

    public abstract class PubSubSpecs
    {
        protected static Context Context1;
        protected static Context Context2;
        protected static SendSocket<string> Send;
        protected static SubscribeSocketPort<string> Subscriber;
        protected static IFiber ClientFiber;
        protected static string Received;
        protected static ManualResetEvent RcvdSignal;

        public PubSubSpecs()
        {
            Context1 = Context.Create();
            Context2 = Context.Create();
            RcvdSignal = new ManualResetEvent(false);
            ClientFiber = PoolFiber.StartNew();
            Send = new SendSocket<string>(Context1,
                "tcp://*:6001",
                s => Encoding.Unicode.GetBytes(s));
            Subscriber = new SubscribeSocketPort<string>(Context2,
                "tcp://localhost:6001",
                x => Encoding.Unicode.GetString(x));
            Subscriber.SubscribeAll();
        }

        protected void Cleanup()
        {
            RcvdSignal.Dispose();
            ClientFiber.Dispose();
            Send.Dispose();
            Console.WriteLine("Dispose pub");
            Subscriber.Dispose();
            Console.WriteLine("Dispose sub");
            Thread.Sleep(100);
        }
    }
}