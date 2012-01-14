using System;
using Disruptor;

namespace Fibrous.Disruptor
{
    public class MsgHandlerAndUnmarshaller<T> : IEventHandler<MsgEvent<T>>
    {
        private readonly Func<byte[], T> _unmarshaller;
        private readonly Action<T> _handler;

        public MsgHandlerAndUnmarshaller(Func<byte[], T> unmarshaller, Action<T> handler)
        {
            _unmarshaller = unmarshaller;
            _handler = handler;
        }

        public void OnNext(MsgEvent<T> data, long sequence, bool endOfBatch)
        {
            T msg = _unmarshaller(data.MsgBuffer);
            _handler(msg);
        }
    }
}