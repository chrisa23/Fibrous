using System;

namespace Fibrous
{
    public interface IExecutionContext
    {
        void Enqueue(Action action);
    }
}