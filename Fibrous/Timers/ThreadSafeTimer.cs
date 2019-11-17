namespace Fibrous
{
    /// <summary>
    ///     Simple fiber based timer that guarantees thread safety for timer actions.
    /// </summary>
    public sealed class ThreadSafeTimer : ConcurrentComponentBase
    {
        public ThreadSafeTimer(IExecutor executor = null) : base(executor)
        {
        }
    }
}