namespace Fibrous.Fibers
{
    using System;
    using System.Collections.Generic;

    public sealed class ExceptionHandlingExecutor : IExecutor
    {
        private readonly Action<Exception> _callback;

        public ExceptionHandlingExecutor(Action<Exception> callback)
        {
            _callback = callback;
        }

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
    }
}