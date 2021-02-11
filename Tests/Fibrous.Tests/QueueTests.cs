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

            ArrayQueue<Action> queue = new ArrayQueue<Action>(16);
            for (int i = 0; i < 16; i++)
            {
                queue.Enqueue(Noop);
            }

            Assert.IsTrue(queue.IsFull);

            (int count, Action[] actions) = queue.Drain();

            Assert.AreEqual(16, count);
            Assert.IsTrue(actions.Take(count).All(x => x != null));
        }
    }
}
