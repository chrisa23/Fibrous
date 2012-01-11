using System;
using System.Collections.Generic;

namespace Fibrous
{
    public interface IExecutor
    {
        void Execute(IEnumerable<Action> toExecute);
        void Execute(Action toExecute);
    }
}