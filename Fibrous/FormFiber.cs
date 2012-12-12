namespace Fibrous
{
    using System;
    using System.ComponentModel;

    public sealed class FormFiber : GuiFiber
    {
        public FormFiber(Executor executor, ISynchronizeInvoke invoker)
            : base(executor, new FormAdapter(invoker))
        {
        }

        public FormFiber(ISynchronizeInvoke invoker)
            : base(new FormAdapter(invoker))
        {
        }

        public static Fiber StartNew(ISynchronizeInvoke invoker)
        {
            var fiber = new FormFiber(invoker);
            fiber.Start();
            return fiber;
        }

        public static Fiber StartNew(Executor executor, ISynchronizeInvoke invoker)
        {
            var fiber = new FormFiber(executor, invoker);
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