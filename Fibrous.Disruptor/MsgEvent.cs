namespace Fibrous.Disruptor
{
    public class MsgEvent<TRequest>
    {
        //these are only useful for reqrep and a few others
        public byte[] SenderId = new byte[16];
        public byte[] CorrelationId = new byte[16];
        public byte[] MsgBuffer;// = new byte[];
        public int Length;
        public TRequest Message { get; set; }
    }
}