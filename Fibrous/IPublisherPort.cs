namespace Fibrous
{
    /// <summary>
    /// Port for publishing side of messages
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPublisherPort<in T>
    {
        /// <summary>
        /// Publish a message and get a true/false as to whether there are any subscribers
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        bool Publish(T msg);
    }
}