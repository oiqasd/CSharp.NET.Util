using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CSharp.Net.Util
{
    public sealed class TokenTask : TokenBus
    {
        public TokenTask(int capacity = 0)
        {
            SetCapacity(capacity);
            bucket = new Queue<byte>(_capacity);
            var t = new Thread(build);
            t.IsBackground = true;
            t.Priority = ThreadPriority.AboveNormal;
            t.Start();
        }

        public void Wait()
        {
            while (!bucket.Any())
                Task.Delay(9).Wait();
            bucket.Dequeue();
        }

        public new void SetCapacity(int value)
        {
            base.SetCapacity(value);
        }
    }
}
