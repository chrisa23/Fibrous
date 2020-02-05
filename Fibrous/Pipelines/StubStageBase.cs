using System;

namespace Fibrous.Pipelines
{
    public abstract class StubStageBase<TIn, TOut> : StageBase<TIn, TOut>
    {
        private readonly Action<Exception> _errorCallback;
        protected Disposables Disposables = new Disposables();
        protected StubStageBase(Action<Exception> errorCallback = null)
        {
            _errorCallback = errorCallback;

            Disposables.Add(In.Subscribe(OnReceive));
        }

        private void OnReceive(TIn obj)
        {
            try
            {
                Receive(obj);
            }
            catch (Exception e)
            {
                _errorCallback?.Invoke(e);
            }
        }

        public override void Dispose()
        {
            Disposables.Dispose();
        }

        protected abstract void Receive(TIn @in);
    }
}