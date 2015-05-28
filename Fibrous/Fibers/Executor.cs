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

    public sealed class ExceptionHandlingExecutor : Executor
    {
        private readonly Action<Exception> _callback;

        public ExceptionHandlingExecutor(Action<Exception> callback = null)
        {
            _callback = callback;
        }

        public override void Execute(Action toExecute)
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