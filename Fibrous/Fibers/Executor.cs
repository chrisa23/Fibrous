using System;
using System.Collections.Generic;

namespace Fibrous
{
    /// <summary>
    ///     Default executor that simply
    /// </summary>
    public sealed class Executor : IExecutor
    {
        public void Execute(List<Action> toExecute)
        {
            for (var index = 0; index < toExecute.Count; index++)
            {
                var action = toExecute[index];
                Execute(action);
            }
        }

        public void Execute(Action toExecute)
        {
            toExecute();
        }

        public void Execute(int count, Action[] actions)
        {
            for (var i = 0; i < count; i++)
            {
                var action = actions[i];
                Execute(action);
            }
        }
    }
}