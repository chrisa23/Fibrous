using System;

namespace Fibrous
{
    /// <summary>
    ///     Abstraction of handling drained batch and individual execution.  Allows insertion of exception handling, profiling,
    ///     etc.
    /// </summary>
    public interface IExecutor
    {
        void Execute(Action toExecute);
    }
}