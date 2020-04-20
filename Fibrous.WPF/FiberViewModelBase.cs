using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Fibrous.WPF
{
    public abstract class FiberViewModelBase : IHaveFiber, INotifyPropertyChanged
    {
        public IFiber Fiber { get; }

        protected FiberViewModelBase(IFiberFactory factory = null)
        {
            Fiber = factory?.Create(OnError) ?? new DispatcherFiber(OnError);
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
