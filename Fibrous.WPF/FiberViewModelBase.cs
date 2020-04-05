using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Fibrous.WPF.Annotations;

namespace Fibrous.WPF
{
    public abstract class FiberViewModelBase : IHaveFiber, INotifyPropertyChanged
    {
        public IFiber Fiber { get; }

        protected FiberViewModelBase()
        {
            Fiber = new DispatcherFiber(OnError);
        }
        protected FiberViewModelBase(IFiberFactory factory)
        {
            Fiber = factory.Create(OnError);
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
