using System;

namespace Fibrous
{
    /// <summary>
    ///     Default executor that simply
    /// </summary>
    public sealed class Executor : IExecutor
    {
        public void Execute(Action toExecute)
        {
            toExecute();
        }
    }
}