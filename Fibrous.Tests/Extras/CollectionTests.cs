using System.Collections.Generic;
using System.Threading;
using Fibrous.Collections;
using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    public class CollectionTests
    {
        [Test]
        public void FiberCollectionTest1()
        {
            int[] snapshot = null;
            var list = new List<int>();
            var collection = new FiberCollection<int>();
            var receive = new Fiber();
            collection.Add(1);
            collection.Add(2);
            collection.Subscribe(receive,
                action =>
                {
                    if (action.ActionType == ActionType.Add)
                        list.Add(action.Items[0]);
                    else
                        list.Remove(action.Items[0]);
                },
                ints => snapshot = ints);
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
            var receive = new Fiber();
            collection.Add(1);
            collection.Add(2);
            collection.Subscribe(receive,
                action =>
                {
                    if (action.ActionType == ActionType.Add)
                        list.Add(action.Items[0]);
                    else
                        list.Remove(action.Items[0]);
                },
                ints => snapshot = ints);
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