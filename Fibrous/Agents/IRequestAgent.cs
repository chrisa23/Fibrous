namespace Fibrous.Agents
{
    using System;

    /// <summary>
    /// Actor like abstraction for request reply. 
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TReply"></typeparam>
    public interface IRequestAgent<in TRequest, TReply> : IRequestPort<TRequest, TReply>, IDisposable
    {
    }
}