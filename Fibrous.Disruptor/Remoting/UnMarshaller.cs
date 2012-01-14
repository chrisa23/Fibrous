using System;
using Disruptor;

namespace Fibrous.Disruptor
{
    /// <summary>
    /// can be decompression|deserializer or just deserializer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UnMarshaller<T> : IEventHandler<MsgEvent<T>>
    {
        private readonly Func<byte[], T> _unmarshaller;

        public UnMarshaller(Func<byte[], T> unmarshaller)
        {
            _unmarshaller = unmarshaller;
        }

        public void OnNext(MsgEvent<T> data, long sequence, bool endOfBatch)
        {
            data.Message = _unmarshaller(data.MsgBuffer);
        }
    }
}