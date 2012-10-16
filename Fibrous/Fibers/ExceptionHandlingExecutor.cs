using System;
using System.Collections.Generic;

namespace Fibrous.Fibers
{
    public sealed class ExceptionHandlingExecutor : IExecutor
    {
        private readonly Action<Exception> _callback;

        public ExceptionHandlingExecutor(Action<Exception> callback)
        {
            _callback = callback;
        }

        #region IExecutor Members

        public void Execute(IEnumerable<Action> toExecute)
        {
            foreach (Action action in toExecute)
                Execute(action);
        }

        public void Execute(Action toExecute)
        {
            try
            {
                toExecute();
            }
            catch (Exception e)
            {
                _callback(e);
            }
        }

        #endregion
    }
}