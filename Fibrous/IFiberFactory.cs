using System;

namespace Fibrous;

/// <summary>
///     Factory for creating fibers with a supplied exception handler.
/// </summary>
public interface IFiberFactory
{
    IFiber CreateFiber(Action<Exception> errorHandler);
}
