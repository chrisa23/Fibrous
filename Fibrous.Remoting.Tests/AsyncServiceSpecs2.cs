namespace Fibrous.Zmq.Specs.AsyncServiceSpecs2
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;
    using Fibrous.Fibers;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class WhenRequestIsSent : AsyncServiceSpecs.AsyncReqReplyServiceSpecs
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
    public class WhenMultipleRequestsAreSent : AsyncServiceSpecs.AsyncReqReplyServiceSpecs
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
    public class CanSendALotFast : AsyncServiceSpecs.AsyncReqReplyServiceSpecs
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
            Replied.WaitOne(TimeSpan.FromSeconds(200));
            sw.Stop();
            Console.WriteLine("Elapsed: " + sw.ElapsedMilliseconds);

            Reply.Should().BeEquivalentTo(EndReply);
            Cleanup();
        }
    }

    public abstract class AsyncReqReplyServiceSpecs
    {
        protected static AsyncReqReplyService2<string, string> Service;
        protected static AsyncReqReplyClient2<string, string> Client;
        protected static IFiber ClientFiber;
        protected static string Reply;
        protected static ManualResetEvent Replied;

        public AsyncReqReplyServiceSpecs()
        {
            
            Reply = string.Empty;
            Replied = new ManualResetEvent(false);
            ClientFiber = new StubFiber();
            Func<byte[], string> unmarshaller = x => Encoding.Unicode.GetString(x);
            Func<string, string> businessLogic = x => x.ToUpper();
            Func<string, byte[]> marshaller = x => Encoding.Unicode.GetBytes(x);
            Service = new AsyncReqReplyService2<string, string>("tcp://*:9997",
                //   "inproc://test_serviceIn",
                //    "tcp://*:9999", //   "inproc://test_serviceOut",
                unmarshaller,
                businessLogic,
                marshaller);
            Console.WriteLine("Start service");
            ClientFiber.Start();
            Console.WriteLine("Start client fiber");
            Client = new AsyncReqReplyClient2<string, string>("tcp://localhost:9997",
                //   "inproc://test_serviceIn",
                //      "tcp://localhost:9999", //   "inproc://test_serviceOut",
                marshaller,
                unmarshaller);
            Console.WriteLine("Start client");
        }
        protected void Cleanup() 
        {
            ClientFiber.Dispose();
            Console.WriteLine("Dispose client fiber");
            Client.Dispose();
            Console.WriteLine("Dispose client");
            Service.Dispose();
            Console.WriteLine("Dispose service");
            Thread.Sleep(100);
        }
    }
}