namespace Fibrous
{
    using System;

    public interface IExecutionContext
    {
        /// <summary>
        /// Enqueue an Action to be executed
        /// </summary>
        /// <param name="action"></param>
        void Enqueue(Action action);
    }
}