namespace Fibrous
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Default executor that simply 
    /// </summary>
    public sealed class Executor : IExecutor
    {
        public void Execute(List<Action> toExecute)
        {
            for (int index = 0; index < toExecute.Count; index++)
            {
                Action action = toExecute[index];
                Execute(action);
            }
        }

        public void Execute(Action toExecute)
        {
            toExecute();
        }
    }
}