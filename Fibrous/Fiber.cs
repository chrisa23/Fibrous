namespace Fibrous
{
    public static class Fiber
    {
        public static IFiber StartNew() => PoolFiber.StartNew();
        public static IFiber StartNew(IExecutor executor) => PoolFiber.StartNew(executor);
        public static IFiber StartNew(IExecutor executor, int size) => PoolFiber.StartNew(executor, size);
    }
}