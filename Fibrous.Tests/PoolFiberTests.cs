namespace Fibrous.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Fibrous.Fibers;
    using NUnit.Framework;

    [TestFixture]
    public class PoolFiberTests
    {
        [Test]
        public void InOrderExecution()
        {
            using (IFiber fiber = PoolFiber.StartNew())
            {
                int count = 0;
                var reset = new AutoResetEvent(false);
                var result = new List<int>();
                Action command = () =>
                {
                    result.Add(count++);
                    if (count == 100)
                    {
                        reset.Set();
                    }
                };
                for (int i = 0; i < 100; i++)
                {
                    fiber.Enqueue(command);
                }
                Assert.IsTrue(reset.WaitOne(10000, false));
                Assert.AreEqual(100, count);
            }
        }

        [Test]
        public void ExecuteOnlyAfterStart()
        {
            using (var fiber = new PoolFiber())
            {
                var reset = new AutoResetEvent(false);
                fiber.Enqueue(() => reset.Set());
                Assert.IsFalse(reset.WaitOne(1, false));
                fiber.Start();
                Assert.IsTrue(reset.WaitOne(1000, false));
            }
        }
    }
}