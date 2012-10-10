namespace Fibrous.Actors
{
    /// <summary> Interface for actor. </summary>
    ///
    /// <typeparam name="TMsg"> Type of the message. </typeparam>
    public interface IActor<in TMsg> : IPublishPort<TMsg>, IDisposableRegistry
    {
        void Start();
    }
}