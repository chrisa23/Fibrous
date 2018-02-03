namespace Fibrous.Pipeline
{
    using System;

    public class Stage<TIn, TOut> : StageBase<TIn, TOut>
    {
        private readonly Func<TIn, TOut> _f;

        public Stage(Func<TIn, TOut> f, FiberType type = FiberType.Pool, IExecutor executor = null) : base(type, executor)
        {
            _f = f;
        }

        protected override void Receive(TIn @in)
        {
            Out.Publish(_f(@in));
        }
    }
}