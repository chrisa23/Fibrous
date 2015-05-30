namespace Fibrous
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Abstraction of handling drained batch and individual execution.  Allows insertion of exception handling, profiling, etc.
    /// </summary>
    public interface IExecutor
    {
        void Execute(List<Action> toExecute);
        void Execute(Action toExecute);
    }

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

    /// <summary>
    /// IExecutor that handles any exceptions thrown with an optional exception callback 
    /// </summary>
    public sealed class ExceptionHandlingExecutor : IExecutor
    {
        private readonly Action<Exception> _callback;

        public ExceptionHandlingExecutor(Action<Exception> callback = null)
        {
            _callback = callback;
        }

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
            try
            {
                toExecute();
            }
            catch (Exception e)
            {
                if (_callback != null)
                    _callback(e);
            }
        }
    }
}