namespace Fibrous
{
    public static class Fiber
    {
        public static IFiber StartNew()
        {
            return PoolFiber.StartNew();
        }

        public static IFiber StartNew(IExecutor executor)
        {
            return PoolFiber.StartNew(executor);
        }

        public static IFiber StartNew(int size)
        {
            return PoolFiber.StartNew(size);
        }

        public static IFiber StartNew(IExecutor executor, int size)
        {
            return PoolFiber.StartNew(executor, size);
        }

        public static IFiber StartNew(IExecutor executor, int size, IFiberScheduler scheduler)
        {
            return PoolFiber.StartNew(executor, size, scheduler);
        }
    }
}