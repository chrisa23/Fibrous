namespace Fibrous.Remoting.Tests.Async
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;
    using CrossroadsIO;
    using Fibrous.Channels;
    using Fibrous.Fibers;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class WhenRequestIsSent : AsyncReqReplyServiceSpecs
    {
        [Test]
        public void Test()
        {
            Client.SendRequest("test",
                ClientFiber,
                x =>
                {
                    Reply = x;
                    Replied.Set();
                });
            Replied.WaitOne(TimeSpan.FromSeconds(1));
            Reply.Should().BeEquivalentTo("TEST");
            Cleanup();
        }
    }

    [TestFixture]
    public class WhenMultipleRequestsAreSent : AsyncReqReplyServiceSpecs
    {
        [Test]
        public void Test()
        {
            for (int i = 0; i < 100; i++)
            {
                Client.SendRequest("test" + i,
                    ClientFiber,
                    x =>
                    {
                        Reply = x;
                        if (x == "TEST99")
                        {
                            Replied.Set();
                        }
                    });
            }
            Replied.WaitOne(TimeSpan.FromSeconds(10));
            Reply.Should().BeEquivalentTo("TEST99");
            Cleanup();
        }
    }

    [TestFixture]
    public class CanSendALotFast : AsyncReqReplyServiceSpecs
    {
        private const int count = 100000;
        private static readonly string EndReply = "TEST" + (count - 1).ToString();

        [Test]
        public void Test()
        {
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
            {
                Client.SendRequest("test" + i,
                    ClientFiber,
                    x =>
                    {
                        Reply = x;
                        if (x == EndReply)
                        {
                            Replied.Set();
                        }
                    });
            }
            Replied.WaitOne(TimeSpan.FromSeconds(20));
            sw.Stop();
            Console.WriteLine("Elapsed: " + sw.ElapsedMilliseconds);
            Reply.Should().BeEquivalentTo(EndReply);
            Cleanup();
        }
    }

    public abstract class AsyncReqReplyServiceSpecs
    {
        protected static AsyncRequestService<string, string> Service;
        protected static AsyncRequestClient<string, string> Client;
        protected static IFiber ClientFiber;
        protected static IFiber ServerFiber;
        protected static string Reply;
        protected static ManualResetEvent Replied;
        protected static AsyncRequestChannel<string, string> Channel;
        protected static Context ClientContext;
        protected static Context ServerContext;

        public AsyncReqReplyServiceSpecs()
        {
            Reply = string.Empty;
            Replied = new ManualResetEvent(false);
            Console.WriteLine("Start client fiber");
            ClientFiber = PoolFiber.StartNew();
            ClientContext = Context.Create();
            ServerFiber = PoolFiber.StartNew();
            ServerContext = Context.Create();
            Channel = new AsyncRequestChannel<string, string>();
            Channel.SetRequestHandler(ServerFiber, request => request.Reply(request.Request.ToUpper()));
            Func<byte[], int, string> unmarshaller = (x, y) => Encoding.Unicode.GetString(x, 0, y);
            Func<string, byte[]> marshaller = x => Encoding.Unicode.GetBytes(x);
            Service = new AsyncRequestService<string, string>(ServerContext,
                "tcp://*",
                9997,
                unmarshaller,
                Channel,
                marshaller);
            Console.WriteLine("Start service");
            ServerFiber.Add(Service);
            ServerFiber.Add(ServerContext);
            Client = new AsyncRequestClient<string, string>(ClientContext,
                "tcp://localhost",
                9997,
                marshaller,
                unmarshaller,
                1024 * 1024);
            ClientFiber.Add(Client);
            ClientFiber.Add(ClientContext);
            Console.WriteLine("Start client");
        }

        protected void Cleanup()
        {
            ClientFiber.Dispose();
            Console.WriteLine("Dispose client fiber");
            ServerFiber.Dispose();
            Console.WriteLine("Dispose service");
            Thread.Sleep(100);
        }
    }
}