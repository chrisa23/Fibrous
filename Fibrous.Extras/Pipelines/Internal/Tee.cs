using System;

namespace Fibrous.Pipelines
{
    internal class Tee<T> : FiberStageBase<T, T>
    {
        private readonly Action<T> _f;

        public Tee(Action<T> f, Action<Exception> errorCallback = null) : base(errorCallback) => _f = f;

        protected override void Receive(T @in)
        {
            _f(@in);
            Out.Publish(@in);
        }
    }
}
