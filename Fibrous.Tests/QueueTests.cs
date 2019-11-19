using System;
using System.Linq;
using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    public class QueueTests
    {
        [Test]
        public void EnqueueDrain()
        {
            void Noop()
            {
            }

            var queue = new ArrayQueue<Action>(16);
            for (var i = 0; i < 16; i++) 
                queue.Enqueue(Noop);

            Assert.IsTrue(queue.IsFull);

            var (count, actions ) = queue.Drain();
            
            Assert.AreEqual(16, count);
            Assert.IsTrue(actions.All(x => x != null));
        }
    }
}