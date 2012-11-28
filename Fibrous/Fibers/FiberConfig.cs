namespace Fibrous.Fibers
{
    using Fibrous.Scheduling;

    public class FiberConfig
    {
        public static readonly FiberConfig Default = new FiberConfig
        {
            //  Queue = new SimpleQueue(),
            Executor = new DefaultExecutor(),
            FiberScheduler = new TimerScheduler()
        };
        //public IQueue Queue { get; set; }
        public IExecutor Executor { get; set; }
        public IFiberScheduler FiberScheduler { get; set; }

        private FiberConfig()
        {
        }

        public FiberConfig(IExecutor executor)
        {
            Executor = executor;
            FiberScheduler = Default.FiberScheduler;
        }
    }
}