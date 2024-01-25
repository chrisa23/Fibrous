using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fibrous.Collections;
using NUnit.Framework;

namespace Fibrous.Tests;

[TestFixture]
public class CollectionTests
{
    [Test]
    public async Task FiberCollectionTest1()
    {
        int[] snapshot = null;
        List<int> list = new();
        using FiberCollection<int> collection = new();
        using AutoResetEvent reset = new(false);
        using AsyncFiber receive = new();
        collection.Add(1);
        collection.Add(2);
        collection.Subscribe(receive,
            async action =>
            {
                if (action.ActionType == ActionType.Add)
                {
                    list.Add(action.Items[0]);
                }
                else
                {
                    list.Remove(action.Items[0]);
                }

                reset.Set();
            },
            async ints =>
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

        int[] items = await collection.GetItemsAsync(x => true);
        Assert.AreEqual(2, items.Length);
    }

    [Test]
    public async Task KeyCollectionTest1()
    {
        int[] snapshot = null;
        List<int> list = new();
        using FiberKeyedCollection<int, int> collection = new(x => x);
        using AutoResetEvent reset = new(false);
        using AsyncFiber receive = new();
        collection.Add(1);
        collection.Add(2);
        collection.Subscribe(receive,
            async action =>
            {
                if (action.ActionType == ActionType.Add)
                {
                    list.Add(action.Items[0]);
                }
                else
                {
                    list.Remove(action.Items[0]);
                }

                reset.Set();
            },
            async ints =>
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

        int[] items = await collection.GetItemsAsync(x => true);
        Assert.AreEqual(2, items.Length);
    }

    [Test]
    public async Task DictionaryCollectionTest1()
    {
        KeyValuePair<int, int>[] snapshot = null;
        List<KeyValuePair<int, int>> list = new();
        using FiberDictionary<int, int> collection = new();
        using AutoResetEvent reset = new(false);
        using AsyncFiber receive = new();
        collection.Add(1, 1);
        collection.Add(2, 2);
        collection.Subscribe(receive,
            async action =>
            {
                if (action.ActionType == ActionType.Add)
                {
                    list.Add(action.Items[0]);
                }
                else
                {
                    list.Remove(action.Items[0]);
                }

                reset.Set();
            },
            async ints =>
            {
                snapshot = ints;
                reset.Set();
            });

        Assert.IsTrue(reset.WaitOne(2000));

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

        KeyValuePair<int, int>[] items = await collection.GetItemsAsync(x => true);
        Assert.AreEqual(2, items.Length);
        collection.Clear();

        items = await collection.GetItemsAsync(x => true);
        Assert.AreEqual(0, items.Length);
    }

    [Test]
    public async Task DictionaryCollectionTest2()
    {
        using FiberDictionary<int, int> collection = new();
        collection.Add(1, 1);
        collection.Add(2, 2);

        Dictionary<int, int> local = new();

        using AutoResetEvent reset = new(false);
        using AsyncFiber fiber = new();

        //Snapshot after subscribe local copy
        collection.SubscribeLocalCopy(fiber, local, () => reset.Set());

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
        KeyValuePair<int, int>[] items = await collection.GetItemsAsync(x => true);
        Assert.AreEqual(2, items.Length);

        //Clear
        collection.Clear();
        items = await collection.GetItemsAsync(x => true);
        Assert.AreEqual(0, items.Length);
    }
}
