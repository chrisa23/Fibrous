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
            using var collection = new FiberCollection<int>();
            using var reset = new AutoResetEvent(false);
            using var receive = new Fiber();
            collection.Add(1);
            collection.Add(2);
            collection.Subscribe(receive,
                action =>
                {
                    if (action.ActionType == ActionType.Add)
                        list.Add(action.Items[0]);
                    else
                        list.Remove(action.Items[0]);
                    reset.Set();
                },
                ints =>
                {
                    snapshot = ints;
                    reset.Set();
                });
            
            Assert.IsTrue(reset.WaitOne(1000));
            
            Assert.AreEqual(2, snapshot.Length);
            Assert.AreEqual(1, snapshot[0]);
            Assert.AreEqual(2, snapshot[1]);
            Assert.AreEqual(0, list.Count);
            
            collection.Add(3);
            Assert.IsTrue(reset.WaitOne(1000));
            
            Assert.AreEqual(1, list.Count);
            
            collection.Remove(3);
            Assert.IsTrue(reset.WaitOne(1000));
            
            Assert.AreEqual(0, list.Count);
            
            var items = collection.GetItems(x => true);
            Assert.AreEqual(2, items.Length);
        }

        [Test]
        public void KeyCollectionTest1()
        {
            int[] snapshot = null;
            var list = new List<int>();
            using var collection = new FiberKeyedCollection<int, int>(x => x);
            using var reset = new AutoResetEvent(false);
            using var receive = new Fiber();
            collection.Add(1);
            collection.Add(2);
            collection.Subscribe(receive,
                action =>
                {
                    if (action.ActionType == ActionType.Add)
                        list.Add(action.Items[0]);
                    else
                        list.Remove(action.Items[0]);
                    reset.Set();
                },
                ints =>
                {
                    snapshot = ints;
                    reset.Set();
                });
            
            Assert.IsTrue(reset.WaitOne(1000));
            
            Assert.AreEqual(2, snapshot.Length);
            Assert.AreEqual(1, snapshot[0]);
            Assert.AreEqual(2, snapshot[1]);
            Assert.AreEqual(0, list.Count);
            
            collection.Add(3);
            Assert.IsTrue(reset.WaitOne(1000));

            Assert.AreEqual(1, list.Count);
            
            collection.Remove(3);
            Assert.IsTrue(reset.WaitOne(1000));
            
            Assert.AreEqual(0, list.Count);
            
            var items = collection.GetItems(x => true);
            Assert.AreEqual(2, items.Length);
        }

        [Test]
        public void DictionaryCollectionTest1()
        {
            KeyValuePair<int, int>[] snapshot = null;
            var list = new List<KeyValuePair<int, int>>();
            using var collection = new FiberDictionary<int, int>();
            using var reset = new AutoResetEvent(false);
            using var receive = new Fiber();
            collection.Add(1, 1);
            collection.Add(2, 2);
            collection.Subscribe(receive,
                action =>
                {
                    if (action.ActionType == ActionType.Add)
                        list.Add(action.Items[0]);
                    else
                        list.Remove(action.Items[0]);

                    reset.Set();
                },
                ints =>
                {
                    snapshot = ints;
                    reset.Set();
                });

            Assert.IsTrue(reset.WaitOne(1000));

            Assert.AreEqual(2, snapshot.Length);
            Assert.AreEqual(1, snapshot[0].Key);
            Assert.AreEqual(2, snapshot[1].Key);
            Assert.AreEqual(0, list.Count);
            
            collection.Add(3, 3);
            Assert.IsTrue(reset.WaitOne(1000));
            
            Assert.AreEqual(1, list.Count);
            
            collection.Remove(3);
            Assert.IsTrue(reset.WaitOne(1000));
            
            Assert.AreEqual(0, list.Count);
            
            var items = collection.GetItems(x => true);
            Assert.AreEqual(2, items.Length);
            collection.Clear();
            
            items = collection.GetItems(x => true);
            Assert.AreEqual(0, items.Length);
        }

        [Test]
        public void DictionaryCollectionTest2()
        {
            using var collection = new FiberDictionary<int, int>();
            collection.Add(1, 1);
            collection.Add(2, 2);

            var local = new Dictionary<int, int>();

            using var reset = new AutoResetEvent(false);
            using var fiber = new Fiber();
            
            //Snapshot after subscribe local copy
            collection.SubscribeLocalCopy(fiber, local, () =>reset.Set());

            Assert.IsTrue(reset.WaitOne(1000));

            Assert.AreEqual(2, local.Count);
            Assert.AreEqual(1, local[1]);
            Assert.AreEqual(2, local[2]);

            //Add
            collection.Add(3, 3);
            Assert.IsTrue(reset.WaitOne(1000));
            Assert.AreEqual(3, local.Count);

            //Remove
            collection.Remove(3);
            Assert.IsTrue(reset.WaitOne(1000));
            Assert.AreEqual(2, local.Count);

            //GetItems
            var items = collection.GetItems(x => true);
            Assert.AreEqual(2, items.Length);
            
            //Clear
            collection.Clear();
            items = collection.GetItems(x => true);
            Assert.AreEqual(0, items.Length);
        }
    }
}