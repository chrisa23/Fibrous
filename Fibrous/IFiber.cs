namespace Fibrous
{
    /// <summary>
    /// Fibers are execution contexts that use Threads or ThreadPools for work handlers
    /// </summary>
    public interface IFiber : IExecutionContext, IScheduler, IDisposableRegistry
    {
        /// <summary>
        /// Start the fiber's queue
        /// </summary>
        /// <returns></returns>
        void Start();

        /// <summary>
        /// Stop the fiber
        /// </summary>
        void Stop();
    }
}