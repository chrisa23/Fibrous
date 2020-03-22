using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Fibrous.WPF.Annotations;

namespace Fibrous.WPF
{
    public abstract class AsyncWpfConcurrentComponent : IHaveAsyncFiber, INotifyPropertyChanged
    {
        public IAsyncFiber Fiber { get; }

        protected AsyncWpfConcurrentComponent()
        {
            Fiber = new AsyncDispatcherFiber(OnError);
        }
        protected AsyncWpfConcurrentComponent(IFiberFactory factory)
        {
            Fiber = factory.CreateAsync(OnError);
        }

        protected abstract void OnError(Exception obj);

        public void Dispose()
        {
            Fiber?.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}