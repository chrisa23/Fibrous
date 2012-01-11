namespace Fibrous.Disruptor
{
    public class MsgEvent<TRequest>
    {
        //these are only useful for reqrep and a few others
        public byte[] SenderId = new byte[16];
        public byte[] CorrelationId = new byte[16];
        public byte[] MsgBuffer;
        public TRequest Message { get; set; }
    }
}