#if NET
using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 持有命名共享内存映射的生命周期，并以长度前缀 UTF-8 格式读写文本。
    /// </summary>
    [SupportedOSPlatform("windows")]
    public sealed class SharedMemory : IDisposable
    {
        private const int LengthPrefixSize = sizeof(int);
        private readonly MemoryMappedFile _mapping;
        private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);
        private bool _disposed;

        public SharedMemory(string name, int capacity = 1024)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Shared memory name is required.", nameof(name));
            if (capacity < LengthPrefixSize)
                throw new ArgumentOutOfRangeException(nameof(capacity), $"Capacity must be at least {LengthPrefixSize} bytes.");

            Name = name;
            Capacity = capacity;
            _mapping = MemoryMappedFile.CreateOrOpen(name, capacity, MemoryMappedFileAccess.ReadWrite);
        }

        public string Name { get; }
        public int Capacity { get; }

        public async Task WriteAsync(string value, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            byte[] data = Encoding.UTF8.GetBytes(value ?? string.Empty);
            if (data.Length > Capacity - LengthPrefixSize)
                throw new ArgumentOutOfRangeException(nameof(value),
                    $"UTF-8 payload is {data.Length} bytes; the mapping allows at most {Capacity - LengthPrefixSize} bytes.");

            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                using (MemoryMappedViewStream stream = _mapping.CreateViewStream(0, Capacity, MemoryMappedFileAccess.Write))
                {
                    stream.Position = LengthPrefixSize;
                    await stream.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
                    byte[] length = new byte[LengthPrefixSize];
                    BinaryPrimitives.WriteInt32LittleEndian(length, data.Length);
                    stream.Position = 0;
                    await stream.WriteAsync(length, 0, length.Length, cancellationToken).ConfigureAwait(false);
                    await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                _gate.Release();
            }
        }

        public async Task<string> ReadAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                using (MemoryMappedViewStream stream = _mapping.CreateViewStream(0, Capacity, MemoryMappedFileAccess.Read))
                {
                    byte[] lengthBuffer = await ReadExactAsync(stream, LengthPrefixSize, cancellationToken).ConfigureAwait(false);
                    int length = BinaryPrimitives.ReadInt32LittleEndian(lengthBuffer);
                    if (length < 0 || length > Capacity - LengthPrefixSize)
                        throw new InvalidDataException($"Shared memory payload length {length} is invalid for capacity {Capacity}.");
                    if (length == 0)
                        return string.Empty;

                    byte[] data = await ReadExactAsync(stream, length, cancellationToken).ConfigureAwait(false);
                    return Encoding.UTF8.GetString(data);
                }
            }
            finally
            {
                _gate.Release();
            }
        }

        private static async Task<byte[]> ReadExactAsync(Stream stream, int length, CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[length];
            int offset = 0;
            while (offset < length)
            {
                int read = await stream.ReadAsync(buffer, offset, length - offset, cancellationToken).ConfigureAwait(false);
                if (read == 0)
                    throw new EndOfStreamException("The shared memory payload ended unexpectedly.");
                offset += read;
            }
            return buffer;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SharedMemory));
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            _mapping.Dispose();
            _gate.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
#endif
