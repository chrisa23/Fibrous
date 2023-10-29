using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Fibrous.WPF;

public abstract class FiberViewModelBase : INotifyPropertyChanged
{
    protected FiberViewModelBase(IFiberFactory factory = null) =>
        Fiber = factory?.CreateFiber(OnError) ?? new DispatcherFiber(OnError);

    protected IFiber Fiber { get; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected abstract void OnError(Exception obj);

    public void Dispose() => Fiber?.Dispose();

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
