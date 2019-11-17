namespace Fibrous
{
    public static class Fiber
    {
        public static IFiber StartNew() => PoolFiber.StartNew();

        public static IFiber StartNew(IExecutor executor) => PoolFiber.StartNew(executor);
        
        public static IFiber StartNew(int size) => PoolFiber.StartNew(size);

        public static IFiber StartNew(IExecutor executor, int size) => PoolFiber.StartNew(executor, size);

        public static IFiber StartNew(IExecutor executor, int size, IFiberScheduler scheduler) => PoolFiber.StartNew(executor, size, scheduler);
    }
}