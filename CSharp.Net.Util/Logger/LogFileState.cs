using System;
using System.IO;

namespace CSharp.Net.Util.Logger
{
    internal sealed class LogFileState : IDisposable
    {
        public FileStream Stream { get; private set; }
        public DateTime Date;
        public int LastWriteTick;
        //public StreamWriter Writer { get; private set; }

        private readonly string _dir;
        private readonly LogLevel _level;
        private int _index;

        public LogFileState(string root, LogLevel level, DateTime date)
        {
            _level = level;
            Date = date;

            _dir = Path.Combine(root, level.ToString());
            Directory.CreateDirectory(_dir);

            OpenNew();
        }

        private void OpenNew()
        {
            var path = Path.Combine(
                _dir,
                $"{Date:yyyy-MM-dd}{(_index <= 0 ? "" : ($"_{_index}"))}.log"
            );

            Stream = new FileStream(
                path,
                FileMode.Append,
                FileAccess.Write,
                FileShare.ReadWrite | FileShare.Delete,
                64 * 1024,
                FileOptions.SequentialScan
            );

            //Writer = new StreamWriter(Stream, new UTF8Encoding(false))
            //{
            //    AutoFlush = false
            //};
        }

        public void Rotate(bool MaxFileSize = false)
        {
            Dispose();
            if (MaxFileSize) _index++;
            OpenNew();
        }

        public void Dispose(bool flush)
        {
            try { Stream?.Flush(flush); } catch { }
            try { Stream?.Dispose(); } catch { }
        }

        public void Dispose()
        {
            Dispose(false);
        }
    }
}
