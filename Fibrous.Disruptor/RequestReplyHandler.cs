using System;
using Disruptor;

namespace Fibrous.Disruptor
{
    public class RequestReplyHandler<TRequest, TReply> : IEventHandler<MsgEvent<TRequest>>
    {
        private readonly Func<TRequest, TReply> _logic;
        private readonly RingBuffer<MsgEvent<TReply>> _outBuffer;

        public RequestReplyHandler(Func<TRequest, TReply> logic, RingBuffer<MsgEvent<TReply>> outBuffer)
        {
            _logic = logic;
            _outBuffer = outBuffer;
        }

        public void OnNext(MsgEvent<TRequest> data, long sequence, bool endOfBatch)
        {
            TReply reply = _logic(data.Message);

            PublishReply(data, reply);
        }

        private void PublishReply(MsgEvent<TRequest> data, TReply reply)
        {
            long seq = _outBuffer.Next();
            MsgEvent<TReply> item = _outBuffer[seq];
            Buffer.BlockCopy(data.SenderId, 0, item.SenderId, 0, 16);
            Buffer.BlockCopy(data.CorrelationId, 0, item.CorrelationId, 0, 16);
            item.Message = reply;
            _outBuffer.Publish(seq);
        }
    }
}