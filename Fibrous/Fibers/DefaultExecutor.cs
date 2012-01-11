using System;
using System.Collections.Generic;

namespace Fibrous.Fibers
{
    internal sealed class DefaultExecutor : IExecutor
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