using System;
using System.Threading;
using Fibrous.Fibers;

using NUnit.Framework;

namespace Fibrous.Tests
{
    using Fibrous.Scheduling;

    [TestFixture]
    public class TimerActionTests
    {
        [Test]
        public void Cancel()
        {
            int executionCount = 0;
            Action action = () => executionCount++;
            var timer = new TimerScheduler.TimerAction(new StubFiber(), action, TimeSpan.FromMilliseconds(2));
            Thread.Sleep(100);
            Assert.AreEqual(1, executionCount);
            timer.Dispose();
            Thread.Sleep(150);

            Assert.AreEqual(1, executionCount);
        }


        [Test]
        public void CallbackFromIntervalTimerWithCancel()
        {
            int executionCount = 0;
            Action action = () => executionCount++;
            var timer = new TimerScheduler.TimerAction(new StubFiber(), action, TimeSpan.FromMilliseconds(2), TimeSpan.FromMilliseconds(150));
            Thread.Sleep(100);
            Assert.AreEqual(1, executionCount);
            timer.Dispose();
            Thread.Sleep(150);

            Assert.AreEqual(1, executionCount);
        }
    }
}