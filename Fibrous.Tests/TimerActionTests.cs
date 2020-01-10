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
            var executionCount = 0;

            void Action()
            {
                executionCount++;
            }

            using (var stubFiber = new StubFiber())
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
            var executionCount = 0;

            void Action()
            {
                executionCount++;
            }

            var timer = new TimerAction(new StubFiber(), Action, TimeSpan.FromMilliseconds(2));
            Thread.Sleep(100);
            Assert.AreEqual(1, executionCount);
            timer.Dispose();
            Thread.Sleep(150);
            Assert.AreEqual(1, executionCount);
        }
    }
}