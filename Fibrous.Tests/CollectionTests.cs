using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fibrous.Tests
{
    using System.Threading;
    using Fibrous.Collections;
    using NUnit.Framework;

    [TestFixture]
    public class CollectionTests
    {
        [Test]
        public void FiberCollectionTest1()
        {
            int[] snapshot = null;
            var list = new List<int>();
            var collection = new FiberCollection<int>();
            var receive = Fiber.StartNew(FiberType.Pool);
            collection.Add(1);
            collection.Add(2);
            collection.Subscribe(receive,
                action =>
                {
                    if (action.ActionType == ActionType.Add)
                        list.Add(action.Item);
                    else
                        list.Remove(action.Item);
                }, ints => snapshot = ints);
            Thread.Sleep(10);
            Assert.AreEqual(2, snapshot.Length);
            Assert.AreEqual(1, snapshot[0]);
            Assert.AreEqual(2, snapshot[1]);
            Assert.AreEqual(0, list.Count);
            collection.Add(3);
            Thread.Sleep(10);
            Assert.AreEqual(1, list.Count);
            collection.Remove(3);
            Thread.Sleep(10);
            Assert.AreEqual(0, list.Count);
            var items = collection.GetItems(x => true);
            Assert.AreEqual(2, items.Length);

        }

        [Test]
        public void KeyCollectionTest1()
        {
            int[] snapshot = null;
            var list = new List<int>();
            var collection = new FiberKeyedCollection<int, int>(x => x);
            var receive = Fiber.StartNew(FiberType.Pool);
            collection.Add(1);
            collection.Add(2);
            collection.Subscribe(receive,
                action =>
                {
                    if (action.ActionType == ActionType.Add)
                        list.Add(action.Item);
                    else
                        list.Remove(action.Item);
                }, ints => snapshot = ints);
            Thread.Sleep(10);
            Assert.AreEqual(2, snapshot.Length);
            Assert.AreEqual(1, snapshot[0]);
            Assert.AreEqual(2, snapshot[1]);
            Assert.AreEqual(0, list.Count);
            collection.Add(3);
            Thread.Sleep(10);
            Assert.AreEqual(1, list.Count);
            collection.Remove(3);
            Thread.Sleep(10);
            Assert.AreEqual(0, list.Count);
            var items = collection.GetItems(x => true);
            Assert.AreEqual(2, items.Length);
        }
    }
}
