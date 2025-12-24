using System;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.Net.Util.LiteSockets
{
    public interface IMessageNode
    {
        ValueTask SendAsync<T>(string typeId, T obj, CancellationToken ct = default);
        void OnMessage<T>(string typeId, Func<T, Task> handler);
    }
}
