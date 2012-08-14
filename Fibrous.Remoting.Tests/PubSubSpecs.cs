namespace Fibrous.Zmq.Specs.PubSubSpecs
{
    using System;
    using System.Text;
    using System.Threading;
    using CrossroadsIO;
    using Fibrous.Fibers;
    using Fibrous.Remoting;
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
            Publisher.Publish("test");
            RcvdSignal.WaitOne(TimeSpan.FromSeconds(1));
            Received.Should().BeEquivalentTo("test");
            Cleanup();
        }
    }

    public abstract class PubSubSpecs
    {
        protected static Context Context1;
        protected static Context Context2;
        protected static PublisherSocket<string> Publisher;
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
            Publisher = new PublisherSocket<string>(Context1,
                "tcp://*:6001",
                (s, socket) => socket.Send(Encoding.Unicode.GetBytes(s)));
            Subscriber = new SubscribeSocketPort<string>(Context2,
                "tcp://localhost:6001",
                socket => socket.Receive(Encoding.Unicode));

            Subscriber.SubscribeAll();
        }

        protected void Cleanup()
        {
            RcvdSignal.Dispose();
            ClientFiber.Dispose();
            Publisher.Dispose();
            Console.WriteLine("Dispose pub");
            Subscriber.Dispose();
            Console.WriteLine("Dispose sub");
            Thread.Sleep(100);
        }
    }
}