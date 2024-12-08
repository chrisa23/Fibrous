using System.Threading;

namespace Fibrous;

internal struct SingleShotGuard
{
    private const int NotCalled = 0;
    private const int Called     = 1;
    private       int _state;

    public bool Check => Interlocked.Exchange(ref _state, Called) == NotCalled;
}
