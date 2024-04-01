using System;

namespace CSharp.Net.Util
{
    class SystemLog : ISystemLog
    {
        public SystemLog()
        {
            LoggerTime = DateTime.Now;
            Level = LogLevel.Info;
        }

        public SystemLog(LogLevel level, string title, string message, DateTime? loggerTime, string loggerName,
            string exception = null, string transferId = null, string appId = null, long elapsedTime = 0)
        {
            TransferId = transferId;
            AppId = appId;
            Title = title;
            Message = message;
            LoggerName = loggerName;
            Exception = exception;
            LoggerTime = loggerTime;
            ElapsedTime = elapsedTime;
            Level = level;
        }

        public string TransferId { get; set; }
        public string AppId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
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
