using System;
using System.Collections.Generic;

namespace Fibrous
{
    /// <summary>
    ///     Abstraction of handling drained batch and individual execution.  Allows insertion of exception handling, profiling,
    ///     etc.
    /// </summary>
    public interface IExecutor
    {
        void Execute(Action toExecute);
        void Execute(int count, Action[] actions);
    }
}