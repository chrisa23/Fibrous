using System;
using System.Collections.Generic;

namespace Fibrous.Pipelines
{
    //Should this use a stub fiber?
    public class Batch<T> : StageFiberBase<T, T[]>
    {
        private readonly List<T> _batch = new List<T>();
        
        public Batch(TimeSpan time, Action<Exception> errorCallback):base(errorCallback)
        {
            Fiber.Schedule(Flush, time, time);
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
            _batch.Add(@in);
        }
    }
}