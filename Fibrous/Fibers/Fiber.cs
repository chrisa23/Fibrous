namespace Fibrous
{
    using System;

    public static class Fiber
    {
        private static IFiber GetFromTyoe(FiberType type)
        {
            switch (type)
            {
                case FiberType.Thread:
                    return new ThreadFiber();
                case FiberType.Pool:
                    return new PoolFiber();
                case FiberType.Stub:
                    return new StubFiber();
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        public static IFiber StartNew(FiberType type)
        {
            var fiber = GetFromTyoe(type);
            fiber.Start();
            return fiber;
        }
    }
}