using System;
using System.Threading.Tasks;

namespace Fibrous.Actors
{
    public abstract class AsyncUntypedActor : IDisposable
    {
        private readonly IRequestPort<object, object> _askChannel;
        private readonly IChannel<object> _tellChannel;
        protected IAsyncFiber Fiber;

        protected AsyncUntypedActor(IFiberFactory factory = null)
        {
            Fiber = factory?.CreateAsyncFiber(OnError) ?? new AsyncFiber(OnError);
            _tellChannel = Fiber.NewChannel<object>(ReceiveAsync);
            _askChannel = Fiber.NewRequestPort<object, object>(OnRequestAsync);
        }

        public void Dispose() => Fiber.Dispose();

        private Task OnRequestAsync(IRequest<object, object> request)
        {
            request.Reply(Reply(request.Request));
            return Task.CompletedTask;
        }

        protected abstract object Reply(object request);
        protected abstract Task ReceiveAsync(object o);
        protected abstract void OnError(Exception obj);

        public void Tell(object message) => _tellChannel.Publish(message);

        public async Task<object> AskAsync(object message) => await _askChannel.SendRequestAsync(message);
    }
}
