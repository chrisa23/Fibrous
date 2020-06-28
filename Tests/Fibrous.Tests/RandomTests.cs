using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Fibrous.Agents;
using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    class RandomTests
    {
        private const int OperationsPerInvoke = 100000;
        private IChannel<string> _input = new Channel<string>();
        private IChannel<string> _queue = new QueueChannel<string>();
        private IChannel<string> _output = new Channel<string>();

        [Test]
        public void FanOutIn()
        {
            int count = 0;
            using var reset = new AutoResetEvent(false);
            void Handler1(string x)
            {
                var u = x.ToUpper();
                _queue.Publish(u);
            }
            void Handler(string x)
            {
                var l = x.ToLower();
                _output.Publish(l);
            }
            void Action(string x)
            {
                count++;
                if (count >= OperationsPerInvoke) reset.Set();
            }

            using var d = new Disposables();
            using var fiber = new ChannelAgent<string>(_input, Handler1);
            for (int i = 0; i < 10; i++)
            {
                d.Add(new ChannelAgent<string>(_queue, Handler));
            }
            using var fiberOut = new ChannelAgent<string>(_output, Action);

            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                _input.Publish("a");
            }

            Assert.IsTrue(reset.WaitOne(TimeSpan.FromSeconds(20)));
        }

        [Test]
        public void GuardTest()
        {
            var singleShot = new SingleShotGuard();
            Assert.IsTrue(singleShot.Check);
            Assert.IsFalse(singleShot.Check);
        }
    }
}

