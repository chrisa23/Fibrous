using System;
using System.Threading;

namespace Fibrous
{
    internal struct AggressiveSpinWait
    {
        private static readonly bool IsSingleProcessor = Environment.ProcessorCount == 1;
        private const int YieldThreshold = 10;
        private const int Sleep0EveryHowManyTimes = 5;
        private int _count;

        private bool NextSpinWillYield => _count > YieldThreshold || IsSingleProcessor;

        public void SpinOnce()
        {
            if (NextSpinWillYield)
            {
                var yieldsSoFar = _count >= YieldThreshold ? _count - YieldThreshold : _count;

                if (yieldsSoFar % Sleep0EveryHowManyTimes == Sleep0EveryHowManyTimes - 1)
                    Thread.Sleep(0);
                else
                    Thread.Yield();
            }
            else
            {
                Thread.SpinWait(4 << _count);
            }

            _count = _count == int.MaxValue ? YieldThreshold : _count + 1;
        }
    }
}