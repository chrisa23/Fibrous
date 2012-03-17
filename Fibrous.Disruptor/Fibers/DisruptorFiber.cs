using System;
using System.Threading.Tasks;
using Disruptor;
using Disruptor.Dsl;
using Fibrous.Fibers;

namespace Fibrous.Disruptor.Fibers
{
    public sealed class DisruptorFiber : FiberBase
    {
        private sealed class EventHandler : IEventHandler<ActionEvent>
        {
            public void OnNext(ActionEvent data, long sequence, bool endOfBatch)
            {
                Action action = data.Action;
                data.Action = null;
                action();
            }
        }

        private sealed class ActionEvent
        {
            public Action Action { get; set; }
        }

        private readonly IClaimStrategy _claimStrategy;
        private readonly IWaitStrategy _waitStrategy;
        private readonly IEventHandler<ActionEvent> _eventHandler;

        private readonly Disruptor<ActionEvent> _disruptor;
        private RingBuffer<ActionEvent> _ringBuffer;

        public DisruptorFiber()
            : this(new MultiThreadedLowContentionClaimStrategy((int)Math.Pow(2, 16)), new YieldingWaitStrategy())
        {
            _disruptor = new Disruptor<ActionEvent>(
                () => new ActionEvent(), _claimStrategy, _waitStrategy, TaskScheduler.Default);
            _disruptor.HandleEventsWith(_eventHandler);

        }

        public DisruptorFiber(IClaimStrategy claimStrategy, IWaitStrategy waitStrategy)
        {
            _claimStrategy = claimStrategy;
            _waitStrategy = waitStrategy;
            _eventHandler = new EventHandler();
        }

        public override void Start()
        {
            _ringBuffer = _disruptor.Start();
        }

        public override void Enqueue(Action action)
        {
            if (_ringBuffer == null)
                return;

            long seq = _ringBuffer.Next();
            var item = _ringBuffer[seq];
            item.Action = action;
            _ringBuffer.Publish(seq);
        }

        protected override void Dispose(bool disposing)
        {
            _disruptor.Shutdown();
            base.Dispose(disposing);
        }


    }
}