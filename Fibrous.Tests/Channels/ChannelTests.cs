using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Retlang.Channels;
using Retlang.Core;
using Retlang.Fibers;

namespace RetlangTests.Channels
{
    [TestFixture]
    public class ChannelTests
    {
        [Test]
        public void PubSub()
        {
            Channel<string> channel = new Channel<string>();
            SynchronousCommandQueue queue = new SynchronousCommandQueue();
            Assert.IsFalse(channel.Publish("hello"));
            bool received = false;
            Action<string> onReceive = delegate(string data)
                                           {
                                               Assert.AreEqual("hello", data);
                                               received = true;
                                           };
            channel.Subscribe(queue, onReceive);
            Assert.IsTrue(channel.Publish("hello"));
            Assert.IsTrue(received);
            channel.ClearSubscribers();
            Assert.IsFalse(channel.Publish("hello"));
        }

        [Test]
        public void PubSubFilterTest()
        {
            Channel<int> channel = new Channel<int>();
            SynchronousCommandQueue queue = new SynchronousCommandQueue();
            int received = 0;
            Action<int> onReceive = delegate(int data)
                                        {
                                            Assert.IsTrue(data%2 == 0);
                                            received++;
                                        };
            ChannelSubscription<int> subber = new ChannelSubscription<int>(queue, onReceive);
            subber.FilterOnProducerThread = delegate(int msg) { return msg%2 == 0; };
            channel.SubscribeOnProducerThreads(subber);
            for (int i = 0; i <= 4; i++)
            {
                channel.Publish(i);
            }
            Assert.AreEqual(3, received);
        }

        [Test]
        public void PubSubUnsubscribe()
        {
            Channel<string> channel = new Channel<string>();
            SynchronousCommandQueue queue = new SynchronousCommandQueue();
            bool received = false;
            Action<string> onReceive = delegate(string data)
                                           {
                                               Assert.AreEqual("hello", data);
                                               received = true;
                                           };
            IUnsubscriber unsub = channel.Subscribe(queue, onReceive);
            Assert.IsTrue(channel.Publish("hello"));
            Assert.IsTrue(received);
            unsub.Dispose();
            Assert.IsFalse(channel.Publish("hello"));
            unsub.Dispose();
        }

        [Test]
        public void SubToBatch()
        {
            Channel<string> channel = new Channel<string>();
            StubCommandContext queue = new StubCommandContext();
            bool received = false;
            Action<IList<string>> onReceive = delegate(IList<string> data)
                                                  {
                                                      Assert.AreEqual(5, data.Count);
                                                      Assert.AreEqual("0", data[0]);
                                                      Assert.AreEqual("4", data[4]);
                                                      received = true;
                                                  };
            channel.SubscribeToBatch(queue, onReceive, 0);

            for (int i = 0; i < 5; i++)
            {
                channel.Publish(i.ToString());
            }
            Assert.AreEqual(1, queue.Scheduled.Count);
            queue.Scheduled[0]();
            Assert.IsTrue(received);
            queue.Scheduled.Clear();
            received = false;

            channel.Publish("5");
            Assert.IsFalse(received);
            Assert.AreEqual(1, queue.Scheduled.Count);
        }

        [Test]
        public void SubToKeyedBatch()
        {
            Channel<KeyValuePair<string, string>> channel = new Channel<KeyValuePair<string, string>>();
            StubCommandContext queue = new StubCommandContext();
            bool received = false;
            Action<IDictionary<string, KeyValuePair<string, string>>> onReceive =
                delegate(IDictionary<string, KeyValuePair<string, string>> data)
                    {
                        Assert.AreEqual(2, data.Keys.Count);
                        Assert.AreEqual(data["0"], new KeyValuePair<string, string>("0", "4"));
                        Assert.AreEqual(data["1"], new KeyValuePair<string, string>("1", "3"));
                        received = true;
                    };
            Converter<KeyValuePair<string, string>, string> key =
                delegate(KeyValuePair<string, string> pair) { return pair.Key; };
            channel.SubscribeToKeyedBatch(queue, key, onReceive, 0);

            for (int i = 0; i < 5; i++)
            {
                channel.Publish(new KeyValuePair<string, string>((i%2).ToString(), i.ToString()));
            }
            Assert.AreEqual(1, queue.Scheduled.Count);
            queue.Scheduled[0]();
            Assert.IsTrue(received);
            queue.Scheduled.Clear();
            received = false;

            channel.Publish(new KeyValuePair<string, string>("1", "1"));
            Assert.IsFalse(received);
            Assert.AreEqual(1, queue.Scheduled.Count);
        }

        [Test]
        public void SubscribeToLast()
        {
            Channel<int> channel = new Channel<int>();
            StubCommandContext queue = new StubCommandContext();
            bool received = false;
            int lastReceived = -1;
            Action<int> onReceive = delegate(int data)
                                        {
                                            lastReceived = data;
                                            received = true;
                                        };
            channel.SubscribeToLast(queue, onReceive, 0);

            for (int i = 0; i < 5; i++)
            {
                channel.Publish(i);
            }
            Assert.AreEqual(1, queue.Scheduled.Count);
            Assert.IsFalse(received);
            Assert.AreEqual(-1, lastReceived);
            queue.Scheduled[0]();
            Assert.IsTrue(received);
            Assert.AreEqual(4, lastReceived);
            queue.Scheduled.Clear();
            received = false;
            lastReceived = -1;
            channel.Publish(5);
            Assert.IsFalse(received);
            Assert.AreEqual(1, queue.Scheduled.Count);
            queue.Scheduled[0]();
            Assert.IsTrue(received);
            Assert.AreEqual(5, lastReceived);
        }
        
        [Test]
        public void AsyncRequestReplyWithPrivateChannelUsingThreads()
        {
            IFiber responder = new ThreadFiber();
            responder.Start();
            IFiber receiver = new ThreadFiber();
            receiver.Start();

            Channel<Channel<string>> requestChannel = new Channel<Channel<string>>();
            Channel<string> replyChannel = new Channel<string>();
            AutoResetEvent reset = new AutoResetEvent(false);
            Action<Channel<string>> onRequest = delegate(Channel<string> reply) { reply.Publish("hello"); };
            requestChannel.Subscribe(responder, onRequest);
            Action<string> onMsg = delegate(string msg)
                                       {
                                           Assert.AreEqual("hello", msg);
                                           reset.Set();
                                       };
            replyChannel.Subscribe(receiver, onMsg);
            Assert.IsTrue(requestChannel.Publish(replyChannel));
            Assert.IsTrue(reset.WaitOne(10000, false));

            responder.Dispose();
            receiver.Dispose();
        }

        [Test, Explicit]
        public void PointToPointPerfTest()
        {
            Channel<int> channel = new Channel<int>();
            IFiber bus = new PoolFiber();
            bus.Start();
            int max = 5000000;
            AutoResetEvent reset = new AutoResetEvent(false);
            Action<int> onMsg = delegate(int count)
                                    {
                                        if (count == max)
                                        {
                                            reset.Set();
                                        }
                                    };
            channel.Subscribe(bus, onMsg);
            using (new PerfTimer(max))
            {
                for (int i = 0; i <= max; i++)
                {
                    channel.Publish(i);
                }
                Assert.IsTrue(reset.WaitOne(30000, false));
            }
        }

        [Test]
        public void BasicPubSubWithPoolQueue()
        {
            IFiber queue = new PoolFiber();
            queue.Start();
            Channel<string> hello = new Channel<string>();
            Channel<string> hello2 = new Channel<string>();

            AutoResetEvent reset = new AutoResetEvent(false);
            Action<string> receiveHello = delegate(string str)
                                              {
                                                  Assert.AreEqual("hello", str);
                                                  reset.Set();
                                              };
            hello.Subscribe(queue, receiveHello);
            hello2.Subscribe(queue, receiveHello);
            Assert.IsTrue(hello.Publish("hello"));
            Assert.IsTrue(reset.WaitOne(10000, false));
            queue.Dispose();
        }
    }

    public class StubCommandContext : IScheduler
    {
        public List<Command> Scheduled = new List<Command>();

        public ITimerControl Schedule(Command command, long timeTilEnqueueInMs)
        {
            Scheduled.Add(command);
            return null;
        }

        public ITimerControl ScheduleOnInterval(Command command, long firstInMs, long regularInMs)
        {
            Scheduled.Add(command);
            return null;
        }
    }
}