using System;
using System.Threading;

namespace CSharp.Net.Util
{
    public class SystemLog : ISystemLog
    {
        public SystemLog()
        {
            LoggerTime = DateTime.Now;
            Level = LogLevel.Info;
        }

        //internal static ThreadLocal<string> _eventId = new ThreadLocal<string>(() => Guid.NewGuid().ToString("N"));
        /*
          internal static AsyncLocal<string> _eventId = new AsyncLocal<string>(AsyncLocalValueChanged);
          private static void AsyncLocalValueChanged(AsyncLocalValueChangedArgs<string> obj)
          => Console.WriteLine($"AsyncLocalValueChanged_{obj.PreviousValue}_{obj.CurrentValue},thread:{Thread.CurrentThread.ManagedThreadId}");
        */

        public SystemLog(LogLevel level, string title, string message, DateTime? loggerTime, string loggerName,
            Exception exception = null, string eventId = null, string appId = null, long elapsedTime = 0)
        {
            EventId = eventId;
            AppId = appId;
            Title = title;
            Message = message;
            LoggerName = loggerName;
            Exception = exception;
            LoggerTime = loggerTime;
            ElapsedTime = elapsedTime;
            Level = level;
        }

        public string EventId { get; set; }
        public string AppId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
        public DateTime? LoggerTime { get; set; }
        public long ElapsedTime { get; set; }
        public LogLevel Level { get; set; }
        /// <summary>
        /// 文件名
        /// </summary>
        public string LoggerName { get; set; }

        public override string ToString()
        {
            return JsonHelper.Serialize(this);
        }
    }
}
