namespace Fibrous.Fibers
{
    using System;
    using System.ComponentModel;

    public sealed class FormFiber : GuiFiberBase
    {
        public FormFiber(ISynchronizeInvoke invoker, IExecutor executor)
            : base(new FormAdapter(invoker), executor)
        {
        }

        public FormFiber(ISynchronizeInvoke invoker)
            : base(new FormAdapter(invoker), new DefaultExecutor())
        {
        }

        public static IFiber StartNew(ISynchronizeInvoke invoker)
        {
            var fiber = new FormFiber(invoker);
            fiber.Start();
            return fiber;
        }

        public static IFiber StartNew(ISynchronizeInvoke invoker, IExecutor executor)
        {
            var fiber = new FormFiber(invoker, executor);
            fiber.Start();
            return fiber;
        }

        private class FormAdapter : IExecutionContext
        {
            private readonly ISynchronizeInvoke _invoker;

            public FormAdapter(ISynchronizeInvoke invoker)
            {
                _invoker = invoker;
            }

            public void Enqueue(Action action)
            {
                _invoker.BeginInvoke(action, null);
            }
        }
    }
}