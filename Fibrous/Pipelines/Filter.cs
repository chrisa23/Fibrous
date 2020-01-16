﻿using System;

namespace Fibrous.Pipelines
{
    public class Filter<T> : StageStubFiberBase<T, T>
    {
        private readonly Predicate<T> _f;

        public Filter(Predicate<T> f, Action<Exception> errorCallback = null) : base(errorCallback)
        {
            _f = f;
        }

        protected override void Receive(T @in)
        {
            if(_f(@in))
                Out.Publish(@in);
        }
    }
}