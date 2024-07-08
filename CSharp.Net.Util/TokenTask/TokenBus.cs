using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CSharp.Net.Util
{
    public abstract class TokenBus
    {
        internal Queue<byte> bucket = null;
        internal int _capacity = 50;
        const int MaxCap = 1000000;
        const int MinCap = 1;

        internal void build()
        {
            while (true)
            {
                int cnt = bucket.Count;
                if (cnt < _capacity)
                    while (cnt++ < _capacity)
                        bucket.Enqueue(0);
                Thread.Sleep(1000);
            }
        }

        internal void SetCapacity(int value)
        {
            if (value < MinCap) return;
            if (value > MaxCap) return;
            _capacity = value;
            if (bucket != null && value < (bucket.Count() / 3))
                bucket = new Queue<byte>(_capacity);
            //bucket.Clear();
        }
    }
}
