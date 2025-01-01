namespace Fibrous;

/// <summary>
///     Port for publishing side of messages
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IPublisherPort<in T>
{
    /// <summary>
    ///     Publish a message
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    void Publish(T msg);
}
