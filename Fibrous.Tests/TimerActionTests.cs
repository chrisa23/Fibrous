namespace Fibrous.Tests
{
    using System;
    using System.Threading;
    using Fibrous.Fibers;
    using Fibrous.Scheduling;
    using NUnit.Framework;

    [TestFixture]
    public class TimerActionTests
    {
        [Test]
        public void CallbackFromIntervalTimerWithCancel()
        {
            int executionCount = 0;
            void Action() => executionCount++;
            using (IFiber stubFiber = StubFiber.StartNew())
            {
                var timer = new TimerAction(stubFiber,
                    Action,
                    TimeSpan.FromMilliseconds(2),
                    TimeSpan.FromMilliseconds(150));
                Thread.Sleep(100);
                Assert.AreEqual(1, executionCount);
                timer.Dispose();
                Thread.Sleep(150);
                Assert.AreEqual(1, executionCount);
            }
        }

        [Test]
        public void Cancel()
        {
            int executionCount = 0;
            void Action() => executionCount++;
            var timer = new TimerAction(StubFiber.StartNew(), Action, TimeSpan.FromMilliseconds(2));
            Thread.Sleep(100);
            Assert.AreEqual(1, executionCount);
            timer.Dispose();
            Thread.Sleep(150);
            Assert.AreEqual(1, executionCount);
        }
    }
}