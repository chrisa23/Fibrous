using System;
using System.Threading.Tasks;
using Disruptor;
using Disruptor.Dsl;
using Fibrous.Fibers;

namespace Fibrous.Disruptor.Fibers
{
    public class DisruptorFiber : FiberBase
    {
        private readonly IClaimStrategy _claimStrategy;
        private readonly IWaitStrategy _waitStrategy;
        private readonly IEventHandler<ActionEvent> _eventHandler;

        private Disruptor<ActionEvent> _disruptor;
        private RingBuffer<ActionEvent> _ringBuffer;

        public DisruptorFiber():this(new MultiThreadedLowContentionClaimStrategy((int)Math.Pow(2,16)),new YieldingWaitStrategy(), new DefaultExecutor() )
        {
            
        }

        public DisruptorFiber(IClaimStrategy claimStrategy, IWaitStrategy waitStrategy, IExecutor executor)
        {
            _claimStrategy = claimStrategy;
            _waitStrategy = waitStrategy;
            _eventHandler = new EventHandler(executor);
        }

        public override void Start()
        {
            _disruptor = new Disruptor<ActionEvent>(
                () => new ActionEvent(), _claimStrategy, _waitStrategy, TaskScheduler.Default);
            _disruptor.HandleEventsWith(_eventHandler);
            _ringBuffer = _disruptor.Start();
        }

        public override void Enqueue(Action action)
        {
            long seq = _ringBuffer.Next();
            var item = _ringBuffer[seq];
            item.Action = action;
            _ringBuffer.Publish(seq);
        }

        public override void Dispose()
        {
            _disruptor.Shutdown();
            base.Dispose();
        }
    }
}