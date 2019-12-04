using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Fibrous
{
    internal static class Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Action<T> Receive<T>(this IExecutionContext fiber, Action<T> receive)
        {
            //how to avoid this closure...
            return msg => fiber.Enqueue(() => receive(msg));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Action<T> Receive<T>(this IAsyncExecutionContext fiber, Func<T, Task> receive)
        {
            //how to avoid this closure...
            return msg => fiber.Enqueue(() => receive(msg));
        }
    }
}
