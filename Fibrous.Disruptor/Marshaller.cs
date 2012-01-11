using System;
using Disruptor;

namespace Fibrous.Disruptor
{
    /// <summary>
    /// can be serializer|compression or just serializer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Marshaller<T> : IEventHandler<MsgEvent<T>>
    {
        private readonly Func<T, byte[]> _marshaller;

        public Marshaller(Func<T, byte[]> marshaller)
        {
            _marshaller = marshaller;
        }

        public void OnNext(MsgEvent<T> data, long sequence, bool endOfBatch)
        {
            data.MsgBuffer = _marshaller(data.Message);
        }
    }
}