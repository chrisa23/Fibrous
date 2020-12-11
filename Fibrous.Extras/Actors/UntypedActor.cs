using System;
using System.Threading.Tasks;

namespace Fibrous.Actors
{
    public abstract class UntypedActor : IDisposable
    {
        private readonly IRequestPort<object, object> _askChannel;
        private readonly IChannel<object> _tellChannel;
        protected IFiber Fiber;

        protected UntypedActor(IFiberFactory factory = null)
        {
            Fiber = factory?.CreateFiber(OnError) ?? new Fiber(OnError);
            _tellChannel = Fiber.NewChannel<object>(Receive);
            _askChannel = Fiber.NewRequestPort<object, object>(OnRequest);
        }

        public void Dispose() => Fiber.Dispose();

        private void OnRequest(IRequest<object, object> request) => request.Reply(Reply(request.Request));

        protected abstract object Reply(object request);
        protected abstract void Receive(object o);
        protected abstract void OnError(Exception obj);

        public void Tell(object message) => _tellChannel.Publish(message);

        public async Task<object> Ask(object message) => await _askChannel.SendRequestAsync(message);
    }
}
