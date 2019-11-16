namespace Fibrous.Pipeline
{
    using System;

    public class Tee<T> : StageBase<T, T>
    {
        private readonly Action<T> _f;

        public Tee(Action<T> f, IExecutor executor = null) : base( executor)
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