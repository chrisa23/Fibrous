namespace Fibrous
{
    /// <summary>
    ///   Enqueues pending actions for the context of execution (thread, pool of threads, message pump, etc.)
    /// </summary>
    public interface IFiber : IExecutionContext, IScheduler, IDisposableRegistry
    {
        /// <summary>   Starts the fiber. </summary>
        void Start();

        //pause?
        //Dispose is shutdown
    }

    //fluent setup?
    //IFIber Start()
    //SetScheduler(IFiberSchduler ).Start()
}