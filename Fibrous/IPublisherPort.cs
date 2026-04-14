namespace Fibrous;

/// <summary>
///     Port for publishing messages.
/// </summary>
public interface IPublisherPort<in T>
{
    /// <summary>
    ///     Publishes a message.
    /// </summary>
    /// <param name="message">Message to publish.</param>
    void Publish(T message);
}
