namespace Fibrous
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Abstraction of handling drained batch and individual execution.  Allows insertion of exception handling, profiling, etc.
    /// </summary>
    public interface IExecutor
    {
        void Execute(List<Action> toExecute);
        void Execute(Action toExecute);
    }
}