namespace Fibrous
{
    /// <summary>
    ///     For use with IEventHub to auto wire events
    ///     Denotes a class which has an async Fiber.
    ///     Must be used in conjunction with IHandleAsync<T>
    /// </summary>
    public interface IHaveAsyncFiber
    {
        IAsyncFiber Fiber { get; }
    }
}