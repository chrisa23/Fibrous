namespace Fibrous.Pipeline
{
    using System;

    public class Tee<T> : StageBase<T, T>
    {
        private readonly Action<T> _f;

        public Tee(Action<T> f, FiberType type = FiberType.Pool, IExecutor executor = null) : base(type, executor)
        {
            _f = f;
        }

        protected override void Receive(T @in)
        {
            _f(@in);
            Out.Publish(@in);
        }
    }
}