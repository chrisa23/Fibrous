namespace Fibrous.Fibers
{
    using System;
    using System.Collections.Generic;

    public sealed class DefaultExecutor : IExecutor
    {
        public void Execute(IEnumerable<Action> toExecute)
        {
            foreach (Action action in toExecute)
            {
                Execute(action);
            }
        }

        public void Execute(Action toExecute)
        {
            toExecute();
        }
    }
}