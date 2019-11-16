using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            ArrayQueue<Action> queue = new ArrayQueue<Action>(16);
            for (int i = 0; i < 16; i++)
            {
                queue.Enqueue(noop);
            }

            Assert.IsTrue(queue.IsFull);
            var (count, actions ) = queue.Drain();
            Assert.AreEqual(16, count);

        }
    }
}
