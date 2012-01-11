using System;
using System.Collections.Generic;
using System.Threading;

namespace Fibrous.Channels
{
    ///// <summary>
    ///// The simplest implementation of a Snapshot channel
    ///// the channel itself holds a list of items and on subscribe passes the list to the subscriber
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    //public class ListChannel<T> : IChannel<T>
    //{
    //    private readonly object _lock = new object();
    //    private readonly List<T> _list = new List<T>();
    //    private readonly IChannel<T> _updateChannel = new Channel<T>();

    //    //subscribe and synchronously get current list.
    //    //this assumes we are on the fiber's thread currently...
    //    public IDisposable Subscribe(IFiber fiber, Action<T> handler)
    //    {
    //        IDisposable disposable;
    //        T[] array;
    //        lock (_lock)
    //        {
    //            disposable = _updateChannel.Subscribe(fiber, handler);
    //            array = _list.ToArray();
    //        }
    //        int length = array.Length;
    //        for (int index = 0; index < length; index++)
    //        {
    //            T item = array[index];
    //            handler(item);
    //        }
    //        return disposable;  
    //    }

    //    public bool Publish(T msg)
    //    {
    //        lock (_lock)
    //        {
    //            _list.Add(msg);
    //            bool publish = _updateChannel.Publish(msg);
    //            Monitor.PulseAll(_lock);
    //            return publish;
    //        }
    //    }

    //}

    //simple snapshots...
    //this one enqueues actions to fiber...
    public sealed class ListChannel<T> : IChannel<T>
    {
        private readonly object _lock = new object();
        private readonly List<T> _list = new List<T>();
        private readonly IChannel<T> _updateChannel = new Channel<T>();

        public IDisposable Subscribe(IFiber fiber, Action<T> handler)
        {
            lock (_lock)
            {
                IDisposable disposable = _updateChannel.Subscribe(fiber, handler);

                int length = _list.Count;
                for (int index = 0; index < length; index++)
                {
                    T item = _list[index];
                    fiber.Enqueue(() => handler(item));
                }

                return disposable;
            }
        }

        public bool Publish(T msg)
        {
            lock (_lock)
            {
                _list.Add(msg);
                bool publish = _updateChannel.Publish(msg);
                Monitor.PulseAll(_lock);
                return publish;
            }
        }
    }
}