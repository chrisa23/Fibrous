using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Fibrous
{
    public class SingleShotGuard
    {
        private static int NOT_CALLED = 0;
        private static int CALLED = 1;
        private int _state = NOT_CALLED;

        public bool Check => Interlocked.Exchange(ref _state, CALLED) == NOT_CALLED;
    }
}
