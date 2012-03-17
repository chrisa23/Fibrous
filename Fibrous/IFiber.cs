namespace Fibrous
{
    /// <summary>
    ///   Enqueues pending actions for the context of execution (thread, pool of threads, message pump, etc.)
    /// </summary>
    public interface IFiber : IStartable, IExecutionContext, IDisposableRegistry
    {
    }
}