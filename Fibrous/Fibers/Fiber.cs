namespace Fibrous
{
    using System;

    public enum FiberType
    {
        Thread,
        Pool,
        Stub
    }

    public static class Fiber
    {
        private static IFiber GetFromTyoe(FiberType type, IExecutor executor)
        {
            switch (type)
            {
                case FiberType.Thread:
                    return new ThreadFiber(executor);
                case FiberType.Pool:
                    return new PoolFiber(executor);
                case FiberType.Stub:
                    return new StubFiber(executor);
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        /// <summary>
        /// Helper to create and start an IFiber by type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="executor"></param>
        /// <returns></returns>
        public static IFiber StartNew(FiberType type, IExecutor executor = null) //TODO:  add Queue
        {
            if (executor == null) executor = new Executor();
            //if(queue == null) queue = new YieldingQueue();
            IFiber fiber = GetFromTyoe(type, executor);
            fiber.Start();
            return fiber;
        }
    }
}