using System;
using System.Threading.Tasks;

namespace Fibrous.Pipelines
{
    internal class AsyncTee<T> : AsyncFiberStageBase<T, T>
    {
        private readonly Func<T, Task> _f;

        public AsyncTee(Func<T, Task> f, Action<Exception> errorCallback = null) : base(errorCallback)
        {
            _f = f;
        }

        protected override async Task Receive(T @in)
        {
            await _f(@in);
            Out.Publish(@in);
        }
    }
}