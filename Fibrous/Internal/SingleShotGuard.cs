using System.Threading;

namespace Fibrous;

internal struct SingleShotGuard
{
    private static readonly int NOT_CALLED = 0;
    private static readonly int CALLED = 1;
    private int _state;

    public bool Check => Interlocked.Exchange(ref _state, CALLED) == NOT_CALLED;
}
