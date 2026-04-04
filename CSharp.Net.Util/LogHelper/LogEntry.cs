using System;

namespace CSharp.Net.Util
{
    struct LogEntry
    {
        public LogEntry()
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

        public LogEntry(LogLevel level, string title, string message,
                        Exception exception = null, string eventId = null,
                        string appId = null, long elapsedTime = 0,
                        PrintType printType=PrintType.Text)
        {
            EventId = eventId;
            AppId = appId;
            Title = title;
            Message = message;
            Exception = exception;
            LoggerTime = DateTime.Now;
            ElapsedTime = elapsedTime;
            Level = level;
            PrintType= printType;
        }

        public string EventId { get; set; }
        public string AppId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
        public DateTime LoggerTime { get; set; }
        public long ElapsedTime { get; set; }
        public LogLevel Level { get; set; }
        public PrintType PrintType { get; set; } = PrintType.Text;
        /// <summary>
        /// 文件名
        /// </summary>
       // public string LoggerName { get; set; }
    }

    enum PrintType
    {
        Text,
        Json
    }
}
