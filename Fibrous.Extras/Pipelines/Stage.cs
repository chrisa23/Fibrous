using System;
using System.Collections.Generic;

namespace Fibrous.Pipelines
{
    public class Stage<TIn, TOut> : FiberStageBase<TIn, TOut>
    {
        private readonly Func<TIn, TOut> _f;
        private readonly Func<TIn, IEnumerable<TOut>> _f2;
        private readonly bool _iterate;

        public Stage(Func<TIn, TOut> f, Action<Exception> errorCallback = null) : base(errorCallback)
        {
            _f = f;
            _iterate = false;
        }

        public Stage(Func<TIn, IEnumerable<TOut>> f, Action<Exception> errorCallback = null) : base(errorCallback)
        {
            _f2 = f;
            _iterate = true;
        }

        protected override void Receive(TIn @in)
        {
            if (_iterate)
            {
                foreach (TOut result in _f2(@in))
                {
                    Out.Publish(result);
                }

                return;
            }


            Out.Publish(_f(@in));
        }
    }
}
