using System;
using System.Text;
using System.Threading;
using Fibrous.Fibers;
using Machine.Specifications;
using ZeroMQ;


namespace Fibrous.Zmq.Specs.PubSubSpecs
{
    [Subject("PubSub")]
    public class WhenMsgIsSentPubSub : PubSubSpecs
    {
        private Because of =
            () =>
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
                };

        private It shouldHaveReceivedMessage = () => Received.ShouldEqual("test");
    }

    public abstract class PubSubSpecs
    {
        protected static ZmqContext Context1;
        protected static ZmqContext Context2;
        protected static PublisherSocketPort<string> Publisher;
        protected static SubscribeSocketPort<string> Subscriber;
        protected static IFiber ClientFiber;
        protected static string Received;
        protected static ManualResetEvent RcvdSignal;

        private Establish context =
            () =>
                {
                    Context1 = ZmqContext.Create();
                    Context2 = ZmqContext.Create();
                    RcvdSignal = new ManualResetEvent(false);
                    ClientFiber = PoolFiber.StartNew();
                    Publisher = new PublisherSocketPort<string>(Context1, "tcp://*:6001",
                                                                (s, socket) => socket.Send(Encoding.Unicode.GetBytes(s)));
                    Subscriber = new SubscribeSocketPort<string>(Context2, "tcp://localhost:6001",
                                                                 socket => socket.Receive(Encoding.Unicode));


                    Subscriber.SubscribeAll();
                };

        protected Cleanup cleanup =
            () =>
                {
                    RcvdSignal.Dispose();
                    ClientFiber.Dispose();

                    Publisher.Dispose();
                    Console.WriteLine("Dispose pub");

                    Subscriber.Dispose();
                    Console.WriteLine("Dispose sub");

                    Thread.Sleep(100);
                };
    }
}