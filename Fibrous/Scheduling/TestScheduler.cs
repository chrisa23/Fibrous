using System;
using System.Collections.Generic;

namespace Fibrous.Scheduling
{
    public sealed class TestScheduler : IScheduler
    {
        private readonly List<StubScheduledAction> _scheduled = new List<StubScheduledAction>();

        public IDisposable Schedule(IFiber fiber, Action action, long firstInMs)
        {
            var toAdd = new StubScheduledAction(action, firstInMs, _scheduled);
            _scheduled.Add(toAdd);
            return toAdd;
        }


        public IDisposable ScheduleOnInterval(IFiber fiber, Action action, long firstInMs, long regularInMs)
        {
            var toAdd = new StubScheduledAction(action, firstInMs, regularInMs, _scheduled);
            _scheduled.Add(toAdd);
            return toAdd;
        }

        public void ExecuteAllScheduled()
        {
            foreach (StubScheduledAction scheduled in _scheduled.ToArray())
            {
                scheduled.Execute();
            }
        }


        public List<StubScheduledAction> Scheduled
        {
            get { return _scheduled; }
        }

        public void Dispose()
        {
            _scheduled.ForEach(x => x.Dispose());
        }
    }
}