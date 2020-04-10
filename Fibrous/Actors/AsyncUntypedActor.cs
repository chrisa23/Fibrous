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
            Fiber = factory?.CreateAsync(OnError) ?? new AsyncFiber(OnError);
            _tellChannel = Fiber.NewChannel<object>(Receive);
            _askChannel = Fiber.NewRequestPort<object,object>(OnRequest);
        }

        private Task OnRequest(IRequest<object, object> request)
        {
            request.Reply(Reply(request.Request));
            return Task.CompletedTask;
        }

        protected abstract object Reply(object request);
        protected abstract Task Receive(object o);
        protected abstract void OnError(Exception obj);

        public void Tell(object message)
        {
            _tellChannel.Publish(message);
        }

        public async Task<object> Ask(object message)
        {
            return await _askChannel.SendRequest(message);
        }
        
        public void Dispose()
        {
            Fiber.Dispose();
        }
    }
}
