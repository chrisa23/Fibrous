namespace Fibrous
{
    /// <summary>
    ///     Simple fiber based timer that guarantees thread safety for timer actions.
    /// </summary>
    public sealed class AsyncThreadSafeTimer : AsyncConcurrentComponentBase
    {
        public AsyncThreadSafeTimer(IAsyncExecutor executor = null) : base(executor)
        {
        }
    }
}