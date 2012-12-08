namespace Fibrous.Remoting.Tests
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;
    using CrossroadsIO;
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
                            Replied.Set();
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
                            Replied.Set();
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
        protected static IRequestHandlerPort<string, string> Service;
        protected static IRequestPort<string, string> Client;
        protected static Fiber ClientFiber;
        protected static Fiber ServerFiber;
        protected static string Reply;
        protected static ManualResetEvent Replied;
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
            Func<byte[], string> unmarshaller = x => Encoding.Unicode.GetString(x);
            Func<string, byte[]> marshaller = x => Encoding.Unicode.GetBytes(x);
            Service = new RequestHandlerSocket<string, string>(ServerContext,
                "tcp://*",
                9997,
                unmarshaller,
                marshaller);
            Service.SetRequestHandler(ServerFiber, request => request.Reply(request.Request.ToUpper()));
            Console.WriteLine("Start service");
            ServerFiber.Add((RequestHandlerSocket<string, string>)Service);
            ServerFiber.Add(ServerContext);
            Client = new AsyncRequestSocket<string, string>(ClientContext,
                "tcp://localhost",
                9997,
                marshaller,
                unmarshaller);
            ClientFiber.Add((AsyncRequestSocket<string, string>)Client);
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