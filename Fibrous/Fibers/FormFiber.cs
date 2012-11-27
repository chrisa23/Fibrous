namespace Fibrous.Fibers
{
    using System;
    using System.ComponentModel;

    public sealed class FormFiber : GuiFiberBase
    {
        public FormFiber(FiberConfig config, ISynchronizeInvoke invoker)
            : base(config, new FormAdapter(invoker))
        {
        }

        public FormFiber(ISynchronizeInvoke invoker)
            : base(FiberConfig.Default, new FormAdapter(invoker))
        {
        }

        public static IFiber StartNew(ISynchronizeInvoke invoker)
        {
            var fiber = new FormFiber(invoker);
            fiber.Start();
            return fiber;
        }

        public static IFiber StartNew(FiberConfig config, ISynchronizeInvoke invoker)
        {
            var fiber = new FormFiber(config, invoker);
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