namespace Fibrous
{
    using System;
    using System.Collections.Generic;

    public class Executor
    {
        public virtual void Execute(List<Action> toExecute)
        {
            for (int index = 0; index < toExecute.Count; index++)
            {
                Action action = toExecute[index];
                Execute(action);
            }
        }

        public virtual void Execute(Action toExecute)
        {
            toExecute();
        }
    }
}