using System;
using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    public class QueueTests
    {
        [Test]
        public void EnquueDrain()
        {
            Action noop = () => { };
            var queue = new ArrayQueue<Action>(16);
            for (var i = 0; i < 16; i++) queue.Enqueue(noop);

            Assert.IsTrue(queue.IsFull);
            var (count, actions ) = queue.Drain();
            Assert.AreEqual(16, count);
        }
    }
}