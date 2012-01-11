using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Retlang.Core;
using Retlang.Fibers;

namespace RetlangTests.Tests
{
    [TestFixture]
    public class PoolFiberTests
    {
        [Test]
        public void InOrderExecution()
        {
            PoolFiber fiber = new PoolFiber(new DefaultThreadPool(), new BatchExecutor());
            fiber.Start();

            int count = 0;
            AutoResetEvent reset = new AutoResetEvent(false);
            List<int> result = new List<int>();
            Command command = delegate
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

        [Test]
        public void ExecuteOnlyAfterStart()
        {
            PoolFiber fiber = new PoolFiber();
            AutoResetEvent reset = new AutoResetEvent(false);
            fiber.Enqueue(delegate { reset.Set(); });
            Assert.IsFalse(reset.WaitOne(1, false));
            fiber.Start();
            Assert.IsTrue(reset.WaitOne(1000, false));
            fiber.Stop();
        }
    }
}