namespace Fibrous
{
    using System;
    using System.Collections.Generic;

    public class Executor
    {
        public virtual void Execute(IEnumerable<Action> toExecute)
        {
            foreach (Action action in toExecute)
                Execute(action);
        }

        public virtual void Execute(Action toExecute)
        {
            toExecute();
        }
    }
}