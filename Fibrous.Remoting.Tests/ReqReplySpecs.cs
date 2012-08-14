namespace Fibrous.Zmq.Specs.ReqReplySpecs
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;
    using CrossroadsIO;
    using Fibrous.Channels;
    using Fibrous.Fibers;
    using Fibrous.Remoting;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class WhenRequestIsSent : ReqReplyServiceSpecs
    {
        [Test]
        public void Test()
        {
            Reply = Client.SendRequest("test", TimeSpan.FromSeconds(2));
            if(Reply == "TEST") Replied.Set();
            WaitHandle.WaitAny(new WaitHandle[] { Replied }, TimeSpan.FromSeconds(1));
            Reply.Should().BeEquivalentTo("TEST");
            Cleanup();
        }
    }

    [TestFixture]
    public class WhenMultipleRequestsAreSent : ReqReplyServiceSpecs
    {
        [Test]
        public void Test()
        {
            for (int i = 0; i < 100; i++)
            {
                Reply = Client.SendRequest("test" + i, TimeSpan.FromSeconds(1));
                if (Reply == "TEST99")
                {
                    Replied.Set();
                }
            }
            WaitHandle.WaitAny(new WaitHandle[] { Replied }, TimeSpan.FromSeconds(1));
            Reply.Should().BeEquivalentTo("TEST99");
            Cleanup();
        }
    }

    [TestFixture]
    public class IsSlowerThanAsyncBy10X : ReqReplyServiceSpecs
    {
        private const string EndReply = "TEST9999";

        [Test]
        
        public void Test()
        {
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < 10000; i++)
            {
                Reply = Client.SendRequest("test" + i, TimeSpan.FromSeconds(1));
                //Console.WriteLine(Reply);
                if (Reply.Equals(EndReply))
                {
                    Console.WriteLine("Set");
                    Replied.Set();
                }
            }
            WaitHandle.WaitAny(new WaitHandle[] { Replied }, TimeSpan.FromSeconds(15));
            Console.WriteLine("Elapsed: " + sw.ElapsedMilliseconds);
            Assert.AreEqual(Reply, EndReply);
            Cleanup();
        }
    }

    public abstract class ReqReplyServiceSpecs
    {
        protected static Context Context;
        protected static Context Context2;

        protected static ReqReplyService<string, string> Service;
        protected static ReqReplyClient<string, string> Client;

        protected static RequestReplyChannel<string,string> _channel = new RequestReplyChannel<string, string>();

        protected static IFiber Fiber = PoolFiber.StartNew();
        protected static string Reply;
        protected static ManualResetEvent Replied = new ManualResetEvent(false);

        public ReqReplyServiceSpecs()
        {
            Context = Context.Create();
            Context2 = Context.Create();
            Func<byte[], int, string> unmarshaller = (x, y) => Encoding.Unicode.GetString(x, 0, y);
            _channel.SetRequestHandler(Fiber, request => request.PublishReply(request.Request.ToUpper()));
            
            Func<string, byte[]> marshaller = x => Encoding.Unicode.GetBytes(x);
            Service = new ReqReplyService<string, string>(Context,
                "tcp://*:9995",
                unmarshaller,
                _channel,
                marshaller,256);
            Console.WriteLine("Start service");
            Client = new ReqReplyClient<string, string>(Context2, "tcp://localhost:9995", marshaller, unmarshaller);

            Fiber.Add(Client);
            Fiber.Add(Context2);

            Fiber.Add(Service);
            Fiber.Add(Context);
            Console.WriteLine("Start client");
        }

        protected void Cleanup()
        {
            Fiber.Dispose();
            
            Console.WriteLine("Dispose");
            
            Thread.Sleep(100);
        }
    }
}