using Disruptor;

namespace Fibrous.Disruptor.Fibers
{
    public class EventHandler : IEventHandler<ActionEvent>
    {
        private readonly IExecutor _executor;

        public EventHandler(IExecutor executor)
        {
            _executor = executor;
        }

        public void OnNext(ActionEvent data, long sequence, bool endOfBatch)
        {
            _executor.Execute(data.Action);
        }
    }
}