using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Fibrous.Internal
{
    internal struct AggressiveSpinWait
    {
        private static readonly bool _isSingleProcessor = Environment.ProcessorCount == 1;
        private const int _yieldThreshold = 10;
        private const int _sleep0EveryHowManyTimes = 5;
        private int _count;

        private bool NextSpinWillYield => _count > _yieldThreshold || _isSingleProcessor;

        public void SpinOnce()
        {
            if (NextSpinWillYield)
            {
                int yieldsSoFar = (_count >= _yieldThreshold ? _count - _yieldThreshold : _count);

                if ((yieldsSoFar % _sleep0EveryHowManyTimes) == (_sleep0EveryHowManyTimes - 1))
                {
                    Thread.Sleep(0);
                }
                else
                {
                    Thread.Yield();
                }
            }
            else
            {
                Thread.SpinWait(4 << _count);
            }

            _count = (_count == int.MaxValue ? _yieldThreshold : _count + 1);
        }
    }
}
