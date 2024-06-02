using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    public class TokenBucket
    {
        static LazyConcurrentDictionary<string, TokenTask> pool = new LazyConcurrentDictionary<string, TokenTask>();

        public static TokenTask Case(string key)
        {
            return pool.GetOrAdd(key, _ => { return new TokenTask(); });
        }
    }
}
