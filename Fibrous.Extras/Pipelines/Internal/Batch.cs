using System;
using System.Collections.Generic;

namespace Fibrous.Pipelines
{
    //Should this use a stub fiber?
    //this should probably not start immediately...
    internal class Batch<T> : FiberStageBase<T, T[]>
    {
        private readonly TimeSpan _time;
        private readonly List<T> _batch = new List<T>();
        IDisposable _sub;
        public Batch(TimeSpan time, Action<Exception> errorCallback):base(errorCallback)
        {
            _time = time;
        }

        private void Flush()
        {
            if (_batch.Count > 0)
            {
                var toSend = _batch.ToArray();
                _batch.Clear();
                Out.Publish(toSend);
            }
        }

        protected override void Receive(T @in)
        {
            if(_sub == null)
                _sub = Fiber.Schedule(Flush, _time, _time);

            _batch.Add(@in);
        }
    }
}