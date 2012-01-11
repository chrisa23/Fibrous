using System;
using System.Threading;
using Fibrous.Fibers;
using Fibrous.Scheduling;
using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    public class TimerActionTests
    {
        [Test]
        public void Cancel()
        {
            int executionCount = 0;
            Action action = () => executionCount++;
            var timer = new TimerScheduler.TimerAction(new StubFiber(), action, 2);
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
            var timer = new TimerScheduler.TimerAction(new StubFiber(), action, 2, 150);
            Thread.Sleep(100);
            Assert.AreEqual(1, executionCount);
            timer.Dispose();
            Thread.Sleep(150);

            Assert.AreEqual(1, executionCount);
        }
    }
}