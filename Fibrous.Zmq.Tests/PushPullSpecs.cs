namespace Fibrous.Zmq.Specs.PushPullSpecs
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;
    using Fibrous.Fibers;
    using Machine.Specifications;
    using ZeroMQ;

    [Subject("PushPull")] //queues
    public class WhenMsgIsSent : PushPullSocketPortSpecs
    {
        private Because of = () =>
        {
            Pull.Subscribe(ClientFiber,
                s =>
                {
                    Received = s;
                    RcvdSignal.Set();
                });
            Push.Send("test");
            RcvdSignal.WaitOne(TimeSpan.FromSeconds(1));
        };
        private It shouldHaveReceivedMessage = () => Received.ShouldEqual("test");
    }

    [Subject("PushPull")] //queues
    public class ShouldSendALotFast : PushPullSocketPortSpecs
    {
        private Because of = () =>
        {
            Pull.Subscribe(ClientFiber,
                s =>
                {
                    Received = s;
                    if (s == "test999999")
                    {
                        RcvdSignal.Set();
                    }
                });
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < 1000000; i++)
            {
                Push.Send("test" + i);
            }
            RcvdSignal.WaitOne(TimeSpan.FromSeconds(10));
            sw.Stop();
            Console.WriteLine("Elapsed: " + sw.ElapsedMilliseconds);
        };
        private It shouldHaveReceivedMessage = () => Received.ShouldEqual("test999999");
    }

    public abstract class PushPullSocketPortSpecs
    {
        protected static ZmqContext Context1;
        protected static ZmqContext Context2;
        protected static PushSocketPort<string> Push;
        protected static PullSocketPort<string> Pull;
        protected static IFiber ClientFiber;
        protected static string Received;
        protected static ManualResetEvent RcvdSignal;
        private Establish context = () =>
        {
            Context1 = ZmqContext.Create();
            Context2 = ZmqContext.Create();
            RcvdSignal = new ManualResetEvent(false);
            ClientFiber = new StubFiber();
            Pull = new PullSocketPort<string>(Context1, "tcp://*:6001", socket => socket.Receive(Encoding.Unicode));
            Push = new PushSocketPort<string>(Context2,
                "tcp://localhost:6001",
                (s, socket) => socket.Send(Encoding.Unicode.GetBytes(s)));
        };
        protected Cleanup cleanup = () =>
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
        };
    }
}