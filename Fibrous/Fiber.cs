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

        public static IFiber StartNew(IExecutor executor, int size)
        {
            return PoolFiber.StartNew(executor, size);
        }
    }
}