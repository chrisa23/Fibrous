using System;
using System.Threading.Tasks;

namespace Fibrous
{
    public interface IEventPort
    {
        IDisposable Subscribe(IFiber fiber, Action receive);

        IDisposable Subscribe(IAsyncFiber fiber, Func< Task> receive);

        IDisposable Subscribe(Action receive);
    }
}