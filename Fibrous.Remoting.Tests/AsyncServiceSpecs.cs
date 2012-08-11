namespace Fibrous.Zmq.Specs.AsyncServiceSpecs
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;
    using Fibrous.Fibers;
    using Fibrous.Remoting;
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
        private const int count = 10000;
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
        protected static AsyncReqReplyService<string, string> Service;
        protected static AsyncReqReplyClient<string, string> Client;
        protected static IFiber ClientFiber;
        protected static string Reply;
        protected static ManualResetEvent Replied;

        public AsyncReqReplyServiceSpecs()
        {
            
            Reply = string.Empty;
            Replied = new ManualResetEvent(false);
            ClientFiber = ThreadFiber.StartNew();
            Console.WriteLine("Start client fiber");
            Func<byte[], int, string> unmarshaller = (x,y) => Encoding.Unicode.GetString(x,0,y);
            Func<string, string> businessLogic = x => x.ToUpper();
            Func<string, byte[]> marshaller = x => Encoding.Unicode.GetBytes(x);
            Service = new AsyncReqReplyService<string, string>("tcp://*",9997,
                //   "inproc://test_serviceIn",
                //    "tcp://*:9999", //   "inproc://test_serviceOut",
                unmarshaller,
                businessLogic,
                marshaller);
            Console.WriteLine("Start service");
            
            
            Client = new AsyncReqReplyClient<string, string>("tcp://localhost",9997,marshaller,unmarshaller,1024*1024);
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