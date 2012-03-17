using System;
using Disruptor;

namespace Fibrous.Disruptor
{
    public class MsgHandler<T> : IEventHandler<MsgEvent<T>>
    {
        private readonly Action<T> _handler;

        public MsgHandler(Action<T> handler)
        {
            _handler = handler;
        }

        public void OnNext(MsgEvent<T> data, long sequence, bool endOfBatch)
        {
            _handler(data.Message);
        }
    }
}