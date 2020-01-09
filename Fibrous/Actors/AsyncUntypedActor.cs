using System;
using System.Threading.Tasks;

namespace Fibrous.Actors
{
    public abstract class AsyncUntypedActor : IDisposable
    {
        private readonly IRequestChannel<object, object> _askChannel = new RequestChannel<object, object>();
        private readonly IChannel<object> _tellChannel = new Channel<object>();
        protected IAsyncFiber Fiber;

        protected AsyncUntypedActor()
        {
            Fiber = AsyncFiber.StartNew(new AsyncExceptionHandlingExecutor(OnError));
            _tellChannel.Subscribe(Fiber, Receive);
            _askChannel.SetRequestHandler(Fiber, OnRequest);
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
