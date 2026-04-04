using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

//#if NET5_0_OR_GREATER
namespace CSharp.Net.Util.Logger
{
    internal class ALogger : IDisposable
    {
        // ========= 参数 =========
        private const string _logDirectory = "logs";
        private const int BatchSize = 8192;
        private const int FlushIntervalMs = 200;
        private const long MaxFileSizeBytes = 10 * 1024 * 1024;//默认最大文件大小为10MB
        private const int MaxQueueSize = 5_000_000;
        private const int InitialBufSize = 1024;
        private const int ReopenIntervalMs = 2000;// 重建2秒
        private const int IdleCloseMs = 10_000; // 10秒释放

        // ========= 字段 =========
        private readonly string _rootDir;
        private readonly ConcurrentQueue<LogPacket> _queue = new();
        private readonly AutoResetEvent _signal = new(false);
        private readonly Thread _writerThread;
        public static ALogger Instance { get; } = new ALogger(Path.Combine(AppContext.BaseDirectory, _logDirectory));

        private volatile bool _running = true;
        private volatile bool _disabled;

        private readonly Dictionary<LogLevel, LogFileState> _files = new();
        private DateTime _currentDate;
        private int _pending;
        private int _lastCleanupTick;

        public ALogger(string rootDir)
        {
            _rootDir = rootDir;
            Directory.CreateDirectory(_rootDir);
            _currentDate = DateTime.Today;
            _lastCleanupTick = Environment.TickCount;

            _writerThread = new Thread(WriteLoop)
            {
                IsBackground = true,
                Name = "ALogger-Worker"
            };
            _writerThread.Start();

            //安全退出
            AppDomain.CurrentDomain.ProcessExit += (_, __) => SafeStop();
            AppDomain.CurrentDomain.UnhandledException += (_, __) => SafeStop();
            Console.CancelKeyPress += (_, __) => SafeStop();
        }

        /// <summary>
        /// 上报日志
        /// </summary>
        /// <param name="logEntry"></param>
        public void Enqueue(LogEntry logEntry)
        {
            if (!_running || _disabled)
                throw new LoggerStopedException();
            if (_queue.Count > MaxQueueSize)
                throw new LoggerMaxQueueException($"Logger queue max length {MaxQueueSize}.");

            _queue.Enqueue(logPacket(logEntry));

            if (Interlocked.Increment(ref _pending) == 1)
            {
                _signal.Set();
            }
        }

        LogPacket logPacket(LogEntry log)
        {
            if (log.PrintType == PrintType.Json)
                return logPacket2Json(log);

            return logPacket2Text(log);
        }

        private LogPacket logPacket2Text(LogEntry log)
        {
            var builder = new Utf8ValueBuilder(InitialBufSize);
            builder.Append(log.LoggerTime);
            builder.Append((byte)' ');
            if (log.Title.IsHasValue())
            {
                builder.Append((byte)'[');
                builder.Append(log.Title);
                builder.Append((byte)']');
            }
            if (log.EventId.IsHasValue())
            {
                builder.Append("[EventId:");
                builder.Append(log.EventId);
                builder.Append((byte)']');
                builder.Append((byte)',');
            }
            if (log.Message.IsHasValue())
            {
                builder.Append(log.Message);
            }
            if (log.Exception != null)
            {
                builder.Append("[Exception:");
                builder.Append(log.Exception.GetType().Name);
                builder.Append("]详情：");
                builder.Append(log.Exception.GetExcetionMessage());
            }
            //builder.Append(log.LoggerTime);
            //if (log.Exception != null)
            //{
            //    offset += Encoding.UTF8.GetBytes($"[Exception:{log.Exception.GetType().Name}]详情：", span[offset..]);
            //    offset += Encoding.UTF8.GetBytes(log.Exception.GetExcetionMessage(), span[offset..]);
            //}

            builder.Append((byte)'\n');
            var p = new LogPacket
            {
                Level = log.Level,
                Buffer = builder.ToArray(),
                Length = builder.Length
            };

            builder.Dispose();
            return p;
        }
        private LogPacket logPacket2Json(LogEntry log)
        {
            var builder = new Utf8ValueBuilder(InitialBufSize);

            builder.Append("{\"level\":\"");
            builder.Append(log.Level.ToString());
            builder.Append((byte)'"');

            builder.Append(",\"log_time\":\"");
            builder.Append(log.LoggerTime);
            builder.Append((byte)'"');

            builder.Append(",\"title\":\"");
            builder.Append(log.Title);
            builder.Append((byte)'"');

            if (log.EventId.IsHasValue())
            {
                builder.Append(",\"EventId\":\"");
                builder.Append(log.EventId);
                builder.Append((byte)'"');
            }
            if (log.Message.IsHasValue())
            {
                if (log.Message.StartsWith("{") && log.Message.EndsWith("}"))
                {
                    builder.Append((byte)',');
                    builder.Append(log.Message.TrimStart('{').TrimEnd('}'));
                }
                else
                {
                    builder.Append(",\"message\":\"");
                    builder.Append(log.Message);
                    builder.Append((byte)'"');
                }
            }
            builder.Append(",\"exception_type\":\"");
            builder.Append(log.Exception?.GetType().Name);
            builder.Append((byte)'"');

            builder.Append(",\"exception_msg\":\"");
            builder.Append(log.Exception?.GetExcetionMessage().Trim("\n".ToCharArray()));
            builder.Append((byte)'"');


            builder.Append((byte)'}');
            builder.Append((byte)'\n');
            var p = new LogPacket
            {
                Level = log.Level,
                Buffer = builder.ToArray(),
                Length = builder.Length
            };

            builder.Dispose();
            return p;
        }

        /// <summary>
        /// 后台写入
        /// </summary>
        private void WriteLoop()
        {
            int lastReopenTick = Environment.TickCount;
            int lastFlushTick = lastReopenTick;

            while (_running)
            {
                _signal.WaitOne(500);

                int processed = 0;
                while (processed < BatchSize && _queue.TryDequeue(out var packet))
                {
                    try
                    {
                        var state = GetFileState(packet.Level);

                        state.Stream.Write(packet.Buffer, 0, packet.Length);
                        state.LastWriteTick = Environment.TickCount;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.GetExcetionMessage());
                    }
                    finally
                    {
                        //ArrayPool<byte>.Shared.Return(packet.Buffer);
                    }

                    processed++;
                }
                Interlocked.Add(ref _pending, -processed);

                if (_files.IsHasValue())
                {
                    int now = Environment.TickCount;

                    if ((now - lastFlushTick) >= FlushIntervalMs)
                    {
                        foreach (var s in _files.Values)
                        {
                            try { s.Stream.Flush(false); }
                            catch { }
                        }
                        lastFlushTick = now;
                    }

                    if ((now - lastReopenTick) >= ReopenIntervalMs)
                    {
                        ReopenAllStreams();
                        lastReopenTick = now;
                    }
                    CleanupIdleFiles();
                }
            }
            FlushAll();
        }

        /// <summary>
        /// 日期更新
        /// </summary>
        private void RotateIfDateChanged()
        {
            var today = DateTime.Today;
            if (today == _currentDate)
                return;

            FlushAll();
            _currentDate = today;
        }

        private void RotateIfNeeded(LogFileState state)
        {
            while (state.Stream.Length > MaxFileSizeBytes)
                state.Rotate(true);
        }

        private LogFileState GetFileState(LogLevel level)
        {
            RotateIfDateChanged();

            if (_files.TryGetValue(level, out var state))
            {
                if (state.Date == _currentDate && state.Stream.CanWrite)
                    return state;

                try { state.Dispose(); } catch { }
            }

            state = new LogFileState(_rootDir, level, _currentDate);
            _files[level] = state;

            RotateIfNeeded(state);
            return state;
        }
        private void CleanupIdleFiles()
        {
            int now = Environment.TickCount;

            if ((now - _lastCleanupTick) < IdleCloseMs)
            {
                return;
            }
            List<LogLevel>? toRemove = null;
            foreach (var kv in _files)
            {
                if (now - kv.Value.LastWriteTick >= IdleCloseMs)
                {
                    try
                    {
                        (toRemove ??= new()).Add(kv.Key);
                    }
                    catch { }
                }
            }

            if (toRemove == null)
                return;
            foreach (var key in toRemove)
            {
                try
                {
                    _files[key].Dispose();
                }
                catch { }
                _files.Remove(key);
            }

        }
        private void ReopenAllStreams()
        {
            foreach (var kv in _files)
            {
                try
                {
                    kv.Value.Rotate();
                }
                catch
                {
                    // reopen 失败时不抛异常，下一轮再试
                }
            }
        }

        private void FlushAll(bool flush = false)
        {
            foreach (var f in _files.Values)
            {
                f.Dispose(flush);
            }
            _files.Clear();
        }

        /* 
         * 暂时注释掉，通过配置文件设置允许记录的日志等级
         * 
         public static IList<LogLevel> AllowLogLists = null;
         static readonly object _lockobj = new object();
         public static string Log_Level { get; set; }
         /// <summary>
         /// 检查是否需要记录此等级的日志
         /// </summary>
         /// <param name="level"></param>
         /// <returns></returns>
         public static bool IsAllowLog(LogLevel level)
         {
             return GetAllowLogLists().Contains(level);
         }
        
         /// <summary>
         /// 通过配置文件设置允许记录的日志等级
         /// </summary>
         /// <returns></returns>
         public static IList<LogLevel> GetAllowLogLists()
         {
             if (AllowLogLists == null)
             {
                 lock (_lockobj)
                 {
                     if (AllowLogLists != null)
                         return AllowLogLists;
                     AllowLogLists = new List<LogLevel>();
                     string logs = Log_Level?.Trim();
                     if (logs.IsNullOrEmpty() || logs == "*")
                     {
                         List<EnumItem> levels = EnumHelper.GetEnumItems(typeof(LogLevel));
                         foreach (var item in levels)
                         {
                             var v = EnumHelper.ParseByNameOrValue<LogLevel>(item.Name);
                             AllowLogLists.Add(v);
                         }
                     }
                     else if (logs.Length > 2)
                     {
                         string[] logArr = Regex.Split(logs, ",");
                         if (logArr.Length > 0)
                         {
                             for (int i = 0; i < logArr.Length; i++)
                             {
                                 try
                                 {
                                     AllowLogLists.Add(EnumHelper.ParseByNameOrValue<LogLevel>(logArr[i], true));
                                 }
                                 catch (Exception ex)
                                 {
                                     LogHelper.Error("日志等级配置有误", logs, ex, nameof(LogHelper));
                                 }
                             }
                         }
                     }
                 }
             }
             return AllowLogLists;
         }  
         /// <summary>
         /// 归档日志文件
         /// </summary>
         /// <param name="logFilePath"></param>
         private void ArchiveLogFile(string logFilePath)
         {
             var archiveDirectory = Path.Combine(_logDirectory, "archive");
             if (!Directory.Exists(archiveDirectory))
             {
                 Directory.CreateDirectory(archiveDirectory);
             }
        
             var archiveFilePath = Path.Combine(archiveDirectory, Path.GetFileName(logFilePath));
             var fileIndex = 1;
        
             // 如果归档文件存在，生成新的文件名
             while (File.Exists(archiveFilePath))
             {
                 archiveFilePath = Path.Combine(archiveDirectory, $"{Path.GetFileNameWithoutExtension(logFilePath)}_{fileIndex}{Path.GetExtension(logFilePath)}");
                 fileIndex++;
             }
        
             File.Move(logFilePath, archiveFilePath);
             CompressLogFile(archiveFilePath);
         }
         /// <summary>
         /// 压缩归档的日志文件
         /// </summary>
         /// <param name="logFilePath"></param>
         private void CompressLogFile(string logFilePath)
         {
             var compressedFilePath = logFilePath + ".gz";
             using (var originalFileStream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
             using (var compressedFileStream = new FileStream(compressedFilePath, FileMode.Create))
             using (var compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
             {
                 originalFileStream.CopyTo(compressionStream);
             }
             File.Delete(logFilePath);
         }
        
         /// <summary>
         /// 删除超过最大保留天数的文件
         /// </summary>
         /// <param name="_maxLogRetentionDays"></param>
         private void DeleteOldLogFiles(int _maxLogRetentionDays = 30)
         {
             var archiveDirectory = Path.Combine(_logDirectory, "archive");
             if (!Directory.Exists(archiveDirectory))
             {
                 return;
             }
        
             var logFiles = Directory.GetFiles(archiveDirectory, "log_*.txt");
             foreach (var logFile in logFiles)
             {
                 var creationTime = File.GetCreationTime(logFile);
                 if ((DateTime.GetTimestamp - creationTime).TotalDays > _maxLogRetentionDays)
                 {
                     File.Delete(logFile);
                 }
             }
         } 
 */
        public void Stop(int timeoutMs = 2000)
        {
            if (!_running)
                return;

            _running = false;
            _signal.Set();
            _writerThread.Join(timeoutMs);
            FlushAll(true);
            _signal.Dispose();
        }
        private void SafeStop()
        {
            try { Stop(); } catch { }
        }

        public void Dispose()
        {
            SafeStop();
        }
    }
}
//#endif