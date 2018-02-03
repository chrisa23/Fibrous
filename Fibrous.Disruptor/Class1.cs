using System;

namespace Fibrous.Disruptor
{
    using System.Threading.Tasks;
    using global::Disruptor;

    public class DisruptorFiber:Disposables, IFiber
    {
        private const int _bufferSize = 1024 * 64;
        
        private readonly RingBuffer<ActionEvent> _ringBuffer = RingBuffer<ActionEvent>.CreateMultiProducer(ActionEvent.EventFactory, _bufferSize, new YieldingWaitStrategy());
        private readonly BatchEventProcessor<ActionEvent> _processor;
        private readonly ISequenceBarrier _sequenceBarrier;
        private readonly IFiberScheduler _fiberScheduler = new TimerScheduler();

        public DisruptorFiber(IExecutor executor = null)
        {
            _sequenceBarrier = _ringBuffer.NewBarrier();
            _processor = new BatchEventProcessor<ActionEvent>(_ringBuffer, _sequenceBarrier, new EventHandler(executor));
        }

        public void Enqueue(Action action)
        {
            long seq = _ringBuffer.Next();
            var evt = _ringBuffer[seq];
            evt.Action = action;
            _ringBuffer.Publish(seq);
        }

        public IDisposable Schedule(Action action, TimeSpan dueTime)
        {
            return _fiberScheduler.Schedule(this, action, dueTime);
        }

        public IDisposable Schedule(Action action, TimeSpan startTime, TimeSpan interval)
        {
            return _fiberScheduler.Schedule(this, action, startTime, interval);
        }

        public void Start()
        {
            Task.Run(() => _processor.Run());
        }

        public void Stop()
        {
            _processor.Halt();
        }
    }

    internal class ActionEvent
    {
        public Action Action { get; set; }

        public static ActionEvent EventFactory()
        {
            return new ActionEvent();
        }
    }

    internal class EventHandler: IEventHandler<ActionEvent>
    {
        private readonly IExecutor _executor;

        public EventHandler(IExecutor executor = null)
        {
            _executor = executor ?? new Executor();
        }

        public void OnEvent(ActionEvent data, long sequence, bool endOfBatch)
        {
            _executor.Execute(data.Action);
            data.Action = null;
        }
    }
}
