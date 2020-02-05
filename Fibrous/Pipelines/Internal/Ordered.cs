using System;
using System.Collections.Generic;
using System.Text;

namespace Fibrous.Pipelines
{
    internal struct Ordered<T>
    {
        public Ordered(long index, T item)
        {
            Index = index;
            Item = item;
        }

        public long Index { get; set; }
        public T Item { get; set; }
    }
}
