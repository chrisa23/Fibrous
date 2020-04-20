using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Fibrous.WPF
{
    public abstract class AsyncFiberViewModelBase : IHaveAsyncFiber, INotifyPropertyChanged
    {
        public IAsyncFiber Fiber { get; }

        protected AsyncFiberViewModelBase(IFiberFactory factory = null)
        {
            Fiber = factory?.CreateAsync(OnError) ?? new AsyncDispatcherFiber(OnError);
        }

        protected abstract void OnError(Exception obj);

        public void Dispose()
        {
            Fiber?.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}