using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    public abstract class TokenBucket
    {
        internal Queue<byte> bucket = null;
        int _capacity = 50;
        internal const int MaxCap = 1000000;
        internal const int MinCap = 1;
        internal void build()
        {
            bucket = bucket ?? new Queue<byte>(_capacity);
            while (true)
            {
                int cnt = bucket.Count;
                if (cnt < _capacity)
                    while (cnt++ < _capacity)
                        bucket.Enqueue(0);
                Task.Delay(1000).Wait();
            }
        }
        internal void setCapacity(int value)
        {
            if (value < MinCap) return;
            if (value > MaxCap) return;
            _capacity = value;
            if (bucket != null && _capacity < (bucket.Count() / 3))
                bucket = new Queue<byte>(_capacity);
            //bucket.Clear();
        }
    }
}
