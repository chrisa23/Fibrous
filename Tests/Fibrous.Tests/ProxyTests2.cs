using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fibrous.Proxy;
using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    public class ProxyTests2
    {
        private readonly string letters = "abcdefghijklmnop";

        [Test]
        public async Task NoFiberErrorAsync()
        {
            Random rnd = new Random();
            using Fiber gen1 = new Fiber();
            using Fiber gen2 = new Fiber();
            using Fiber gen3 = new Fiber();

            using IStateManager stateMgr = FiberProxy<IStateManager>.Create(new StateManager());
            gen1.Schedule(() => stateMgr.Add(letters[rnd.Next(16)].ToString()), TimeSpan.FromMilliseconds(10),
                TimeSpan.FromMilliseconds(30));
            gen2.Schedule(() => stateMgr.Remove(letters[rnd.Next(16)].ToString()), TimeSpan.FromMilliseconds(10),
                TimeSpan.FromMilliseconds(60));
            gen3.Schedule(() => stateMgr.Iterate(), TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(240));

            await Task.Delay(TimeSpan.FromSeconds(10));
        }


        public interface IStateManager : IDisposable
        {
            void Add(string s);
            void Remove(string s);

            void Iterate();
        }

        public class StateManager : IStateManager
        {
            private readonly List<string> _data = new List<string>();

            public void Add(string s) => _data.Add(s);

            public void Remove(string s) => _data.Remove(s);

            public void Iterate()
            {
                foreach (string item in _data)
                {
                    Console.WriteLine(item);
                    Thread.Sleep(20);
                }
            }

            public void Dispose()
            {
            }
        }
    }
}
