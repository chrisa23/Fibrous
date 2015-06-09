namespace Fibrous.Remoting.Tests
{
    using System;
    using System.Text;
    using System.Threading;
    using FluentAssertions;
    using NUnit.Framework;
    using NetMQ;

    [TestFixture]
    public class WhenMsgIsSentPubSub : PubSubSpecs
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
            Thread.Sleep(10);
            Send.Publish("test");
            RcvdSignal.WaitOne(TimeSpan.FromSeconds(1));
            Cleanup();
            Received.Should().BeEquivalentTo("test");
        }
    }

    public abstract class PubSubSpecs
    {
        protected static NetMQContext Context1;
        protected static NetMQContext Context2;
        protected static SendSocket<string> Send;
        protected static SubscribeSocket<string> Subscriber;
        protected static IFiber ClientFiber;
        protected static string Received;
        protected static ManualResetEvent RcvdSignal;
        protected static IChannel<string> Channel = new Channel<string>();

        public PubSubSpecs()
        {
            ClientFiber = PoolFiber.StartNew();
            Context1 = NetMQContext.Create();
            Send = new SendSocket<string>(Context1,
                "tcp://localhost:6001",
                s => Encoding.Unicode.GetBytes(s));
            Context2 = NetMQContext.Create();
            RcvdSignal = new ManualResetEvent(false);
            Subscriber = new SubscribeSocket<string>(Context2,
                "tcp://localhost:6001",
                x => Encoding.Unicode.GetString(x),
                Channel);
            Subscriber.SubscribeAll();
        }

        protected void Cleanup()
        {
            ClientFiber.Enqueue(() =>
            {
                Send.Dispose();
                Context1.Dispose();
                Subscriber.Dispose();
                Context2.Dispose();
            });
            Thread.Sleep(100);
            ClientFiber.Dispose();
        }
    }
}