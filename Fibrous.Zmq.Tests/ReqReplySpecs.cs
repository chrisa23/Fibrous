using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Machine.Specifications;
using ZeroMQ;

namespace Fibrous.Zmq.Specs.ReqReplySpecs
{
    [Subject("ReqReplyService")]
    public class WhenRequestIsSent : ReqReplyServiceSpecs
    {
        private Because of =
            () =>
                {
                    Reply = Client.SendRequest("test", TimeSpan.FromSeconds(1));
                    Replied.Set();

                    WaitHandle.WaitAny(new WaitHandle[] {Replied}, TimeSpan.FromSeconds(1));
                };

        private It shouldHaveAReply = () => Reply.ShouldEqual("TEST");
    }

    [Subject("ReqReplyService")]
    public class WhenMultipleRequestsAreSent : ReqReplyServiceSpecs
    {
        private Because of =
            () =>
                {
                    for (int i = 0; i < 100; i++)
                    {
                        Reply = Client.SendRequest("test" + i, TimeSpan.FromSeconds(1));
                        if (Reply == "TEST99")
                            Replied.Set();
                    }
                    WaitHandle.WaitAny(new WaitHandle[] {Replied}, TimeSpan.FromSeconds(1));
                };

        private It shouldHaveLastReply = () => Reply.ShouldEqual("TEST99");
    }

    [Subject("ReqReplyService")]
    public class IsSlowerThanAsyncBy10X : ReqReplyServiceSpecs
    {
        private const string EndReply = "TEST99999";

        private Because of =
            () =>
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    for (int i = 0; i < 100000; i++)
                    {
                        Reply = Client.SendRequest("test" + i, TimeSpan.FromSeconds(1));
                        if (Reply == EndReply)
                            Replied.Set();
                    }
                    WaitHandle.WaitAny(new WaitHandle[] {Replied});
                    Console.WriteLine("Elapsed: " + sw.ElapsedMilliseconds);
                };

        private It shouldBeFast = () => Reply.ShouldEqual(EndReply);
    }

    public abstract class ReqReplyServiceSpecs
    {
        protected static ZmqContext Context;
        protected static ZmqContext Context2;
        protected static ReqReplyService<string, string> Service;
        protected static ReqReplyClient<string, string> Client;
        protected static string Reply;
        protected static ManualResetEvent Replied = new ManualResetEvent(false);

        private Establish _context =
            () =>
                {
                    Context = ZmqContext.Create();
                    Context2 = ZmqContext.Create();
                    Func<byte[],int , string> unmarshaller = (x,y) => Encoding.Unicode.GetString(x,0,y);
                    Func<string, string> businessLogic = x => x.ToUpper();
                    Func<string, byte[]> marshaller = x => Encoding.Unicode.GetBytes(x);
                    Service = new ReqReplyService<string, string>(Context, "tcp://*:9995", unmarshaller, businessLogic,
                                                                  marshaller);
                    Console.WriteLine("Start service");

                    Client = new ReqReplyClient<string, string>(Context2, "tcp://localhost:9995", marshaller,
                                                                unmarshaller);
                    Console.WriteLine("Start client");
                };

        private Cleanup cleanup =
            () =>
                {
                    Client.Dispose();
                    Console.WriteLine("Dispose client");
                    Service.Dispose();
                    Console.WriteLine("Dispose service");
                    Context2.Dispose();
                    Context.Dispose();
                    Thread.Sleep(100);
                };
    }
}