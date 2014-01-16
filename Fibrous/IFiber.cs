namespace Fibrous
{
    public interface IFiber : IExecutionContext, IScheduler, IDisposableRegistry
    {
        IFiber Start();
        void Stop();
    }
}