namespace Fibrous
{
    using System;

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