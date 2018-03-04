using System;
using System.Collections.Generic;
using System.Text;

namespace Fibrous.Experimental
{
    using System.Threading.Tasks;
    using Fibrous.Channels;
    using Fibrous.Fibers;

    public abstract class UntypedActor
    {
        private readonly IChannel<object> _tellChannel = new Channel<object>();
        private readonly IRequestChannel<object, object> _askChannel = new RequestChannel<object, object>();
        protected IFiber Fiber;

        protected UntypedActor(FiberType type = FiberType.Pool)
        {
            Fiber = Fibrous.Fiber.StartNew(type, new ExceptionHandlingExecutor(OnError));
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
            return await Task.Run(() => _askChannel.SendRequest(message).Receive(TimeSpan.MaxValue));
        }
    }
}
