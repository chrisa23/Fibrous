using System;
using System.ComponentModel;

namespace Fibrous
{
    /// <summary>
    ///     Fiber for use with WinForms.  Handles Invoking actions on the UI thread
    /// </summary>
    public sealed class FormFiber : FiberBase
    {
        private readonly ISynchronizeInvoke _invoker;

        public FormFiber( ISynchronizeInvoke invoker, IExecutor executor = null, IFiberScheduler scheduler = null)
            : base(executor?? new Executor(), scheduler?? new TimerScheduler())
        {
            _invoker = invoker;
        }

        protected override void InternalEnqueue(Action action)
        {
            _invoker.BeginInvoke(action, null);
        }
    }
}