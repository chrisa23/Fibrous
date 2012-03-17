namespace Fibrous
{
    using System;
    using System.Collections.Generic;

    public interface IExecutor
    {
        void Execute(IEnumerable<Action> toExecute);
        void Execute(Action toExecute);
    }
}