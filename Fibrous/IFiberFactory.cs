using System;

namespace Fibrous;

public interface IFiberFactory
{
    IFiber CreateFiber(Action<Exception> errorHandler);
}
