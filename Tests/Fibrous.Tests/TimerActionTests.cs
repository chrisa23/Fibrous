using System;
using System.Threading;
using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    public class TimerActionTests
    {
        [Test]
        public void CallbackFromIntervalTimerWithCancel()
        {
            int executionCount = 0;

            void Action()
            {
                executionCount++;
            }

            using (StubFiber stubFiber = new StubFiber())
            {
                TimerAction timer = new TimerAction(stubFiber,
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

            void Action()
            {
                executionCount++;
            }

            TimerAction timer = new TimerAction(new StubFiber(), Action, TimeSpan.FromMilliseconds(2));
            Thread.Sleep(100);
            Assert.AreEqual(1, executionCount);
            timer.Dispose();
            Thread.Sleep(150);
            Assert.AreEqual(1, executionCount);
        }
    }
}
