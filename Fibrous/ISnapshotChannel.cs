using System;
using System.Threading.Tasks;

namespace Fibrous
{
    public interface ISnapshotChannel<T, TSnapshot> : IPublisherPort<T>, ISnapshotSubscriberPort<T, TSnapshot>, IDisposable
    {
        IDisposable ReplyToPrimingRequest(IFiber fiber, Func<TSnapshot> reply);
        IDisposable ReplyToPrimingRequest(IAsyncFiber fiber, Func<Task<TSnapshot>> reply);
    }
}
