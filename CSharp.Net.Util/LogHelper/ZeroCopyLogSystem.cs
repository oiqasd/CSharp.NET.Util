using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    internal class ZeroCopyLogSystem
    {
        private MemoryMappedFile _memoryMappedFile;
        private MemoryMappedViewAccessor _viewAccessor;
        private long _position;
        private readonly object _lock = new object();
        private const int BufferSize = 4096; // 4KB buffer size

        public ZeroCopyLogSystem(string filePath)
        {
            _memoryMappedFile = MemoryMappedFile.CreateOrOpen(filePath, BufferSize);
            _viewAccessor = _memoryMappedFile.CreateViewAccessor();
            _position = 0;
        }

        public void Log(string message)
        {
            var encodedMessage = Encoding.UTF8.GetBytes(message);
            lock (_lock)
            {
                // Ensure we do not overflow the buffer by checking position and buffer size.
                if (_position + encodedMessage.Length + 1 > BufferSize) // +1 for newline character
                {
                    // Roll over the buffer or handle overflow (e.g., by writing to disk)
                    // For simplicity, we'll just reset the position in this example.
                    _position = 0; // Reset position to start of buffer for simplicity.
                }
                _viewAccessor.Write(_position, encodedMessage, 0, encodedMessage.Length);
                _position += encodedMessage.Length;
                _viewAccessor.Write(_position, new byte[] { (byte)'\n' }, 0, 1); // Add newline character
                _position++; // Move to the next position after the newline character.
            }
        }

        public async Task LogAsync(string message)
        {
            var encodedMessage = Encoding.UTF8.GetBytes(message);
            lock (_lock) // Ensure thread safety for writing to the view accessor.
            {
                if (_position + encodedMessage.Length + 1 > BufferSize) // Check for overflow and handle it if needed.
                {
                    _position = 0; // Reset position for simplicity in this example. In a real application, you might want to roll over or flush to disk.
                }
                await _viewAccessor.WriteAsync(_position, encodedMessage, 0, encodedMessage.Length); // Async write to memory mapped file.
                _position += encodedMessage.Length;
                await _viewAccessor.WriteAsync(_position, new byte[] { (byte)'\n' }, 0, 1); // Add newline character asynchronously.
                _position++; // Move to the next position after the newline character.
            }
        }
    }
}
