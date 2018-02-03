namespace Fibrous.Pipeline
{
    public static class StageExtensions
    {
        public static IStage<T, T1> Connect<T0, T, T1>(this IStage<T0, T> stage1, IStage<T, T1> stage2)
        {
            stage1.Subscribe(stage2.Fiber, stage2.Publish);
            return stage2;
        }
    }
}