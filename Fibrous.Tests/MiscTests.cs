using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fibrous.Tests
{
    using Fibrous.Queues;
    using NUnit.Framework;

    [TestFixture]
    public class MiscTests
    {
        [Test]
        public void DecrementRef()
        {
            int count = 100;
            for (int i = 0; i < 100; i++)
            {
                YieldingQueue.ApplyWaitMethod(ref count);
            }
            Assert.AreEqual(0,count);
        }
    }
}
