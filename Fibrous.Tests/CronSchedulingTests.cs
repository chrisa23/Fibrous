using System;
using System.Collections.Generic;
using System.Text;
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
            int count = 0;
            using var fiber = new Fiber();
            using (var sub = fiber.CronSchedule(() => count++, "0/2 * * 1/1 * ? *"))
                await Task.Delay(TimeSpan.FromSeconds(4.1));
            Assert.AreEqual(2, count);

            await Task.Delay(TimeSpan.FromSeconds(5));
            Assert.AreEqual(2, count);

        }

        [Test]
        public async Task BasicAsyncTest()
        {
            int count = 0;
            using var fiber = new AsyncFiber();
            using (var sub = fiber.CronSchedule( async () => count++, "0/2 * * 1/1 * ? *"))
                await Task.Delay(TimeSpan.FromSeconds(4.1));
            Assert.AreEqual(2, count);

            await Task.Delay(TimeSpan.FromSeconds(5));
            Assert.AreEqual(2, count);

        }
    }
}
