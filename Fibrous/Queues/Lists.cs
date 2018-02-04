namespace Fibrous.Queues
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public static class Lists
    {
        /// <summary>
        /// Swap the references of two lists
        /// </summary>
        /// <param name="a">List a</param>
        /// <param name="b">List b</param>
        public static void Swap(ref List<Action> a, ref List<Action> b)
        {
            List<Action> tmp = a;
            a = b;
            b = tmp;
        }

        public static void Swap2(ref List<Action> a, ref List<Action> b)
        {
            var tmp = Interlocked.Exchange(ref a, b);
            Interlocked.Exchange(ref b, tmp);
        }
    }
}