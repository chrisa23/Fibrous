namespace Fibrous
{
    /// <summary>   Interface for actor. </summary>
    ///
    /// <typeparam name="TMsg">  Type of the message. </typeparam>
    public interface IActor<in TMsg> : IPublisherPort<TMsg>, IDisposableRegistry, IStartable
    {
    }
}