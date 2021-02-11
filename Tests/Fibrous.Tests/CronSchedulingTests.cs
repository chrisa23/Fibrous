using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    public class CronSchedulingTests
    {
        [Test]
        public async Task BasicTest()
        {
            int msWait = 1000 - DateTime.Now.TimeOfDay.Milliseconds - 200;
            if (msWait < 0)
            {
                msWait = msWait + 1000;
            }

            await Task.Delay(msWait);

            int count = 0;

            void Action()
            {
                count++;
            }

            using Fiber fiber = new Fiber();
            using (IDisposable sub = fiber.CronSchedule(Action, "0/2 * * 1/1 * ? *"))
            {
                await Task.Delay(TimeSpan.FromSeconds(4.1));
            }

            Assert.IsTrue(count >= 2);

            await Task.Delay(TimeSpan.FromSeconds(8));
            Assert.IsTrue(count <= 3);
        }

        [Test]
        public async Task BasicAsyncTest()
        {
            int msWait = 1000 - DateTime.Now.TimeOfDay.Milliseconds - 200;
            if (msWait < 0)
            {
                msWait = msWait + 1000;
            }

            await Task.Delay(msWait);

            int count = 0;

            Task Action()
            {
                count++;
                return Task.CompletedTask;
            }

            using AsyncFiber fiber = new AsyncFiber();
            using (IDisposable sub = fiber.CronSchedule(Action, "0/2 * * 1/1 * ? *"))
            {
                await Task.Delay(TimeSpan.FromSeconds(4.1));
            }

            Assert.IsTrue(count >= 2);

            await Task.Delay(TimeSpan.FromSeconds(8));
            Assert.IsTrue(count <= 3);
        }
    }
}
