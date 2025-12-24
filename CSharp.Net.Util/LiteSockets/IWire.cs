using System;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.Net.Util.LiteSockets
{
    public interface IWire: IAsyncDisposable
    {
        ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct);
        ValueTask<int> WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken ct);
    }
}
