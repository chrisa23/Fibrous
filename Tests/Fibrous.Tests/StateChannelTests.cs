using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    public class StateChannelTests
    {
        [Test]
        public void StateChannel()
        {
            using var fiber = new Fiber();
            string result = null;
            var reset = new AutoResetEvent(false);
            void Handle(string s)
            {
                result = s;
                reset.Set();
            }

            var channel = new StateChannel<string>("none");
            var fromSeconds = TimeSpan.FromSeconds(1);
            using (var sub = channel.Subscribe(fiber, Handle))
            {
                Assert.IsTrue(reset.WaitOne(fromSeconds));
                Assert.AreEqual("none", result);
                channel.Publish("one");
                Assert.IsTrue(reset.WaitOne(fromSeconds));
                Assert.AreEqual("one", result);
            }
            result = null;
            channel.Publish("two");
            Thread.Sleep(100);
            Assert.IsNull(result);
            using (var sub = channel.Subscribe(fiber, Handle))
            {
                Assert.IsTrue(reset.WaitOne(fromSeconds));
                Assert.AreEqual("two", result);
                channel.Publish("three");
                Assert.IsTrue(reset.WaitOne(fromSeconds));
                Assert.AreEqual("three", result);
            }
        }

        [Test]
        public void StateChannelNoInit()
        {
            using var fiber = new Fiber();
            string result = null;
            var reset = new AutoResetEvent(false);
            void Handle(string s)
            {
                result = s;
                reset.Set();
            }

            var channel = new StateChannel<string>();
            var fromSeconds = TimeSpan.FromSeconds(0.1);
            using (var sub = channel.Subscribe(fiber, Handle))
            {
                Assert.IsFalse(reset.WaitOne(fromSeconds));
                Assert.IsNull(result);
                channel.Publish("one");
                Assert.IsTrue(reset.WaitOne(fromSeconds));
                Assert.AreEqual("one", result);
            }
            result = null;
            channel.Publish("two");
            Thread.Sleep(100);
            Assert.IsNull(result);
            using (var sub = channel.Subscribe(fiber, Handle))
            {
                Assert.IsTrue(reset.WaitOne(fromSeconds));
                Assert.AreEqual("two", result);
                channel.Publish("three");
                Assert.IsTrue(reset.WaitOne(fromSeconds));
                Assert.AreEqual("three", result);
            }
        }

        [Test]
        public void AsyncStateChannel()
        {
            using var fiber = new AsyncFiber();
            string result = null;
            var reset = new AutoResetEvent(false);
            Task Handle(string s)
            {
                result = s;
                reset.Set();
                return Task.CompletedTask;
            }

            var channel = new StateChannel<string>("none");
            var fromSeconds = TimeSpan.FromSeconds(1);
            using (var sub = channel.Subscribe(fiber, Handle))
            {
                Assert.IsTrue(reset.WaitOne(fromSeconds));
                Assert.AreEqual("none", result);
                channel.Publish("one");
                Assert.IsTrue(reset.WaitOne(fromSeconds));
                Assert.AreEqual("one", result);
            }
            result = null;
            channel.Publish("two");
            Thread.Sleep(100);
            Assert.IsNull(result);
            using (var sub = channel.Subscribe(fiber, Handle))
            {
                Assert.IsTrue(reset.WaitOne(fromSeconds));
                Assert.AreEqual("two", result);
                channel.Publish("three");
                Assert.IsTrue(reset.WaitOne(fromSeconds));
                Assert.AreEqual("three", result);
            }
        }

        [Test]
        public void AsyncStateChannelNoInit()
        {
            using var fiber = new AsyncFiber();
            string result = null;
            var reset = new AutoResetEvent(false);
            Task Handle(string s)
            {
                result = s;
                reset.Set();
                return Task.CompletedTask;
            }

            var channel = new StateChannel<string>();
            var fromSeconds = TimeSpan.FromSeconds(0.1);
            using (var sub = channel.Subscribe(fiber, Handle))
            {
                Assert.IsFalse(reset.WaitOne(fromSeconds));
                Assert.IsNull(result);
                channel.Publish("one");
                Assert.IsTrue(reset.WaitOne(fromSeconds));
                Assert.AreEqual("one", result);
            }
            result = null;
            channel.Publish("two");
            Thread.Sleep(100);
            Assert.IsNull(result);
            using (var sub = channel.Subscribe(fiber, Handle))
            {
                Assert.IsTrue(reset.WaitOne(fromSeconds));
                Assert.AreEqual("two", result);
                channel.Publish("three");
                Assert.IsTrue(reset.WaitOne(fromSeconds));
                Assert.AreEqual("three", result);
            }
        }
    }
}
