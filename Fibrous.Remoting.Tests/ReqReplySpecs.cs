namespace Fibrous.Remoting.Tests.ReqReply
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
    public class WhenRequestIsSent : ReqReplyServiceSpecs
    {
        [Test]
        public void Test()
        {
            Reply = Port.SendRequest("test", TimeSpan.FromSeconds(2));
            if (Reply == "TEST")
                Replied.Set();
            WaitHandle.WaitAny(new WaitHandle[] { Replied }, TimeSpan.FromSeconds(5));
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
                Reply = Port.SendRequest("test" + i, TimeSpan.FromSeconds(1));
                if (Reply == "TEST99")
                    Replied.Set();
            }
            WaitHandle.WaitAny(new WaitHandle[] { Replied }, TimeSpan.FromSeconds(5));
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
                Reply = Port.SendRequest("test" + i, TimeSpan.FromSeconds(1));
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
        protected static IRequestHandlerPort<string, string> Service;
        protected static IRequestPort<string, string> Port;
        protected static IFiber Fiber = PoolFiber.StartNew();
        protected static string Reply;
        protected static ManualResetEvent Replied = new ManualResetEvent(false);

        public ReqReplyServiceSpecs()
        {
            Context = Context.Create();
            Func<byte[], string> unmarshaller = (x) => Encoding.Unicode.GetString(x);
            Func<string, byte[]> marshaller = x => Encoding.Unicode.GetBytes(x);
            Service = new RemoteRequestHandlerPort<string, string>(Context, "tcp://*:9995", unmarshaller, marshaller);
            Console.WriteLine("Start service");
            
            Context2 = Context.Create();
            Port = new RemoteRequestPort<string, string>(Context2, "tcp://localhost:9995", marshaller, unmarshaller);
            Fiber.Add(Replied);
            Fiber.Add((RemoteRequestPort<string, string>)Port);
            Fiber.Add(Context2);
            Fiber.Add(Service.SetRequestHandler(new StubFiber(), request => request.Reply(request.Request.ToUpper())));
            Fiber.Add((RemoteRequestHandlerPort<string, string>)Service);
            Fiber.Add(Context);
            Console.WriteLine("Start client");
        }

        //TODO:  seems to be cleanup issues 
        protected void Cleanup()
        {
            Fiber.Dispose();
            Console.WriteLine("Dispose");
            Thread.Sleep(100);
        }
    }
}