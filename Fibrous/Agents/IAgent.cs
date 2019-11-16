using System;

namespace Fibrous.Agents
{
    /// <summary>
    ///     Actor like abstraction.  Recieves a single type of message directly
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAgent<in T> : IPublisherPort<T>, IDisposable
    {
    }
}