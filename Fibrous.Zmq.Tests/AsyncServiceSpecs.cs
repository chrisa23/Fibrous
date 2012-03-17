using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Fibrous.Fibers;
using Machine.Specifications;

namespace Fibrous.Zmq.Specs.AsyncServiceSpecs
{
    [Subject("AsyncReqReplyService")]
    public class WhenRequestIsSent : AsyncReqReplyServiceSpecs
    {
        private Because of =
            () =>
                {
                    Client.SendRequest("test", ClientFiber, x =>
                        {
                            Reply = x;
                            Replied.Set();
                        });
                    Replied.WaitOne(TimeSpan.FromSeconds(1));
                };

        private It shouldHaveAReply = () => Reply.ShouldEqual("TEST");
    }

    [Subject("AsyncReqReplyService")]
    public class WhenMultipleRequestsAreSent : AsyncReqReplyServiceSpecs
    {
        private Because of =
            () =>
                {
                    for (int i = 0; i < 100; i++)
                    {
                        Client.SendRequest("test" + i, ClientFiber,
                                           x =>
                                               {
                                                   Reply = x;
                                                   if (x == "TEST99")
                                                       Replied.Set();
                                               });
                    }
                    Replied.WaitOne(TimeSpan.FromSeconds(10));
                };

        private It shouldHaveLastReply = () => Reply.ShouldEqual("TEST99");
    }

    [Subject("AsyncReqReplyService")]
    public class CanSendALotFast : AsyncReqReplyServiceSpecs
    {
        private const int count = 1000000;
        private static readonly string EndReply = "TEST" + (count - 1).ToString();

        private Because of =
            () =>
                {
                    Stopwatch sw = Stopwatch.StartNew();

                    for (int i = 0; i < count; i++)
                    {
                        Client.SendRequest("test" + i, ClientFiber,
                                           x =>
                                               {
                                                   Reply = x;
                                                   if (x == EndReply)
                                                       Replied.Set();
                                               });
                    }

                    Replied.WaitOne(TimeSpan.FromSeconds(200));
                    sw.Stop();
                    Console.WriteLine("Elapsed: " + sw.ElapsedMilliseconds);
                };

        private It shouldBeFast = () => Reply.ShouldEqual(EndReply);
    }

    public abstract class AsyncReqReplyServiceSpecs
    {
        protected static AsyncReqReplyService<string, string> Service;
        protected static AsyncReqReplyClient<string, string> Client;
        protected static IFiber ClientFiber;
        protected static string Reply;
        protected static ManualResetEvent Replied;

        private Establish _context =
            () =>
                {
                    Reply = string.Empty;
                    Replied = new ManualResetEvent(false);
                    ClientFiber = new PoolFiber();
                    Func<byte[],int, string> unmarshaller = (x,y) =>Encoding.Unicode.GetString(x,0,y);
                    Func<string, string> businessLogic = x => x.ToUpper();
                    Func<string, byte[]> marshaller = x => Encoding.Unicode.GetBytes(x);
                    Service = new AsyncReqReplyService<string, string>(
                        "tcp://*:9998", //   "inproc://test_serviceIn",
                        "tcp://*:9999", //   "inproc://test_serviceOut",
                        unmarshaller,
                        businessLogic,
                        marshaller);
                    Console.WriteLine("Start service");

                    ClientFiber.Start();
                    Console.WriteLine("Start client fiber");

                    Client = new AsyncReqReplyClient<string, string>(
                        "tcp://localhost:9998", //   "inproc://test_serviceIn",
                        "tcp://localhost:9999", //   "inproc://test_serviceOut",
                        marshaller,
                        unmarshaller,1024);
                    Console.WriteLine("Start client");
                };

        protected Cleanup cleanup =
            () =>
                {
                    ClientFiber.Dispose();
                    Console.WriteLine("Dispose client fiber");

                    Client.Dispose();
                    Console.WriteLine("Dispose client");
                    Service.Dispose();
                    Console.WriteLine("Dispose service");

                    Thread.Sleep(100);
                };
    }
}