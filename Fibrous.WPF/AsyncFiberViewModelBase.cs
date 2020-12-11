using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Fibrous.WPF
{
    public abstract class AsyncFiberViewModelBase : INotifyPropertyChanged
    {
        protected AsyncFiberViewModelBase(IFiberFactory factory = null) =>
            Fiber = factory?.CreateAsyncFiber(OnError) ?? new AsyncDispatcherFiber(OnError);

        protected IAsyncFiber Fiber { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected abstract void OnError(Exception obj);

        public void Dispose() => Fiber?.Dispose();

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
