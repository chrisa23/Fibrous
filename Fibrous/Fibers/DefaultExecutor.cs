using System;
using System.Collections.Generic;

namespace Fibrous.Fibers
{
    public sealed class DefaultExecutor : IExecutor
    {
        #region IExecutor Members

        public void Execute(IEnumerable<Action> toExecute)
        {
            foreach (Action action in toExecute)
                Execute(action);
        }

        public void Execute(Action toExecute)
        {
            toExecute();
        }

        #endregion
    }
}