using System;

namespace Fibrous
{
    /// <summary>
    ///     For use with IEventHub to auto wire events
    ///     Denotes a class which has a regular Fiber.
    ///     Must be used in conjunction with IHandle<T>
    /// </summary>
    public interface IHaveFiber : IDisposable
    {
        IFiber Fiber { get; }
    }
}