using System;
using System.Collections.Generic;

namespace Fibrous.Fibers
{
    public class TestFiber : FiberBase
    {
        private readonly List<Action> _pending = new List<Action>();
        private bool _root = true;

        public override void Start()
        {
        }


        public override void Dispose()
        {
            _pending.Clear();
            base.Dispose();
        }


        public override void Enqueue(Action action)
        {
            if (_root && ExecutePendingImmediately)
            {
                try
                {
                    _root = false;
                    action();
                    ExecuteAllPendingUntilEmpty();
                }
                finally
                {
                    _root = true;
                }
            }
            else
            {
                _pending.Add(action);
            }
        }

        public List<Action> Pending
        {
            get { return _pending; }
        }


        public bool ExecutePendingImmediately { get; set; }


        public void ExecuteAllPendingUntilEmpty()
        {
            while (_pending.Count > 0)
            {
                Action toExecute = _pending[0];
                _pending.RemoveAt(0);
                toExecute();
            }
        }


        public void ExecuteAllPending()
        {
            Action[] copy = _pending.ToArray();
            _pending.Clear();
            foreach (Action pending in copy)
            {
                pending();
            }
        }
    }
}