using System;
using System.Threading.Tasks;

namespace Fibrous.Actors
{
    public abstract class UntypedActor : IDisposable
    {
        private readonly IRequestChannel<object, object> _askChannel = new RequestChannel<object, object>();
        private readonly IChannel<object> _tellChannel = new Channel<object>();
        protected IFiber Fiber;

        protected UntypedActor()
        {
            Fiber = PoolFiber.StartNew(new ExceptionHandlingExecutor(OnError));
            Fiber.Subscribe(_tellChannel, Receive);
            _askChannel.SetRequestHandler(Fiber, OnRequest);
        }

        private void OnRequest(IRequest<object, object> request)
        {
            request.Reply(Reply(request.Request));
        }

        protected abstract object Reply(object request);
        protected abstract void Receive(object o);
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
