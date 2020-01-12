using System;
using System.Collections.Generic;

namespace Fibrous.Pipelines
{
    public class Stage<TIn, TOut> : StageBase<TIn, TOut>
    {
        private readonly Func<TIn, TOut> _f;
        private readonly Func<TIn, IEnumerable<TOut>> _f2;

        public Stage(Func<TIn, TOut> f, Action<Exception> errorCallback = null) : base(errorCallback)
        {
            _f = f;
        }

        public Stage(Func<TIn,IEnumerable<TOut>> f, Action<Exception> errorCallback = null) : base(errorCallback)
        {
            _f2 = f;
        }

        protected override void Receive(TIn @in)
        {
            if (_f != null)
            {
                Out.Publish(_f(@in));
                return;
            }

            foreach (var result in _f2(@in))
            {
                Out.Publish(result);
            }

        }
    }
}