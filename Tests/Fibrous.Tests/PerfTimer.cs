using System;
using System.Diagnostics;

namespace Fibrous.Tests
{
    public sealed class PerfTimer : IDisposable
    {
        private readonly int _count;
        private readonly Stopwatch _stopWatch;

        public PerfTimer(int count)
        {
            _count = count;
            _stopWatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopWatch.Stop();
            long elapsed = _stopWatch.ElapsedMilliseconds;
            Console.WriteLine("Elapsed: " + elapsed + " Actions: " + _count);
            Console.WriteLine("actions/ms: " + _count / elapsed);
        }
    }
}
