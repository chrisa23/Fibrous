﻿using System;
using System.ComponentModel;

namespace Fibrous.Fibers.Gui
{
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