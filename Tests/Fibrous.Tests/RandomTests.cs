using System;
using System.Threading;
using Fibrous.Agents;
using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    internal class RandomTests
    {
        private const int OperationsPerInvoke = 100000;
        private readonly IChannel<string> _input = new Channel<string>();
        private readonly IChannel<string> _queue = new QueueChannel<string>();
        private readonly IChannel<string> _output = new Channel<string>();

        [Test]
        public void FanOutIn()
        {
            int count = 0;
            using AutoResetEvent reset = new(false);

            void Handler1(string x)
            {
                string u = x.ToUpper();
                _queue.Publish(u);
            }

            void Handler(string x)
            {
                string l = x.ToLower();
                _output.Publish(l);
            }

            void Action(string x)
            {
                count++;
                if (count >= OperationsPerInvoke)
                {
                    reset.Set();
                }
            }

            using Disposables d = new();
            using ChannelAgent<string> fiber = new(_input, Handler1);
            for (int i = 0; i < 10; i++)
            {
                d.Add(new ChannelAgent<string>(_queue, Handler));
            }

            using ChannelAgent<string> fiberOut = new(_output, Action);

            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                _input.Publish("a");
            }

            Assert.IsTrue(reset.WaitOne(TimeSpan.FromSeconds(20)));
        }

        [Test]
        public void GuardTest()
        {
            SingleShotGuard singleShot = new();
            Assert.IsTrue(singleShot.Check);
            Assert.IsFalse(singleShot.Check);
        }
    }
}
