using System;
using System.Collections.Generic;

namespace Fibrous
{
    /// <summary>
    ///     IExecutor that handles any exceptions thrown with an optional exception callback
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
            for (var index = 0; index < toExecute.Count; index++)
            {
                var action = toExecute[index];
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
                _callback?.Invoke(e);
            }
        }

        public void Execute(int count, Action[] actions)
        {
            for (var index = 0; index < count; index++)
            {
                var action = actions[index];
                Execute(action);
            }
        }
    }
}