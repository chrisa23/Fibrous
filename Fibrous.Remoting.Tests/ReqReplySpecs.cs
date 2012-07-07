namespace Fibrous.Zmq.Specs.ReqReplySpecs
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;
    using CrossroadsIO;
    
      using NUnit.Framework;
    using FluentAssertions;
    [TestFixture]
    public class WhenRequestIsSent : ReqReplyServiceSpecs
    {
        [Test]
        public void Test()
        {

   
            Reply = Client.SendRequest("test", TimeSpan.FromSeconds(1));
            Replied.Set();
            WaitHandle.WaitAny(new WaitHandle[] { Replied }, TimeSpan.FromSeconds(1));
    Reply.Should().BeEquivalentTo("TEST");
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
        private const string EndReply = "TEST99";
        [Test]
        public void Test()
        {
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < 100; i++)
            {
                Reply = Client.SendRequest("test" + i, TimeSpan.FromSeconds(1));
                Console.WriteLine(Reply);
                if (Reply.Equals(EndReply))
                {
                    Console.WriteLine("Set");
                    Replied.Set();
                }
            }
            WaitHandle.WaitAny(new WaitHandle[] { Replied }, TimeSpan.FromSeconds(15));
            Console.WriteLine("Elapsed: " + sw.ElapsedMilliseconds);
          Assert.AreEqual( Reply,EndReply);
            Cleanup();
        }
    }

    public abstract class ReqReplyServiceSpecs
    {
        protected static Context Context;
        protected static Context Context2;
        protected static ReqReplyService<string, string> Service;
        protected static ReqReplyClient<string, string> Client;
        protected static string Reply;
        protected static ManualResetEvent Replied = new ManualResetEvent(false);

        public ReqReplyServiceSpecs()
        {
            
            Context = Context.Create();
            Context2 = Context.Create();
            Func<byte[], int, string> unmarshaller = (x, y) => Encoding.Unicode.GetString(x, 0, y);
            Func<string, string> businessLogic = x => x.ToUpper();
            Func<string, byte[]> marshaller = x => Encoding.Unicode.GetBytes(x);
            Service = new ReqReplyService<string, string>(Context,
                "tcp://*:9995",
                unmarshaller,
                businessLogic,
                marshaller);
            Console.WriteLine("Start service");
            Client = new ReqReplyClient<string, string>(Context2, "tcp://localhost:9995", marshaller, unmarshaller);
            Console.WriteLine("Start client");
        }
        protected void Cleanup()
        {
            Client.Dispose();
            Console.WriteLine("Dispose client");
            Service.Dispose();
            Console.WriteLine("Dispose service");
            Context2.Dispose();
            Context.Dispose();
            Thread.Sleep(100);
        }
    }
}