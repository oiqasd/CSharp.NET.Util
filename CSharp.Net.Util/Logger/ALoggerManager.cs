using System;
using System.IO;

namespace CSharp.Net.Util.Logger
{
    class ALoggerManager
    {
        private static readonly Lazy<ALoggerManager> _instance = new Lazy<ALoggerManager>(() => new ALoggerManager());
        private ALogger _logger;
        private readonly string _logDirectory;
        public static ALoggerManager Instance => _instance.Value;

        private ALoggerManager()
        {
            _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            _logger = new ALogger(_logDirectory);
        }

        public void Log(string message)
        {
            if (_logger == null)
            {
                _logger = new ALogger(_logDirectory);
            }
            _logger.Enqueue(new LogEntry { Message = message });
        }

        public void Dispose()
        {
            _logger?.Dispose();
        }
    }
}
