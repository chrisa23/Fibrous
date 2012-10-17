namespace Fibrous
{
    using System;

    public interface IExecutionContext
    {
        void Enqueue(Action action);
    }
}