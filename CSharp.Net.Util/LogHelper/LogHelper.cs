using CSharp.Net.Util.Logger;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 客户端写日志帮助文件
    /// </summary>
    public sealed class LogHelper
    {
        /// <summary>
        /// Json格式输出
        /// </summary>
        /// <param name="level"><see cref="LogLevel"/></param>
        /// <param name="jsonStr"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static void Log2Json(LogLevel level, string jsonStr, Exception ex = null)
        {
            var log = new LogEntry(level, null, jsonStr, ex, printType: PrintType.Json);
            WriteLog(log);
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param> 
        public static void Log(LogLevel level, params string[] message)
        {
            var log = new LogEntry(level, null, string.Join(" ", message), null, null);
            WriteLog(log);
        }
        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="level"></param>
        /// <param name="ex"></param>
        /// <param name="message"></param>
        public static void Log(LogLevel level, Exception ex, params string[] message)
        {
            var log = new LogEntry(level, null, string.Join(" ", message), ex);
            WriteLog(log);
        }

        /// <summary>
        /// 记录调试日志
        /// </summary>
        /// <param name="message"></param> 
        public static void Debug(string message)
        {
            var log = new LogEntry(LogLevel.Debug, null, message);
            WriteLog(log);

        }
        /// <summary>
        /// 记录调试日志
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param> 
        /// <param name="eventId"></param>
        public static void Debug(string title, string message, string eventId = null)
        {
            var log = new LogEntry(LogLevel.Debug, title, message, eventId: eventId);
            WriteLog(log);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static void Info(params string[] message)
        {
            var log = new LogEntry(LogLevel.Info, null, string.Join(" ", message));
            WriteLog(log);
        }

        /// <summary>
        /// 记录警告日志
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param> 
        public static void Warn(string title, string message, Exception ex = null)
        {
            var log = new LogEntry(LogLevel.Warn, title, message, ex);
            WriteLog(log);
        }

        /// <summary>
        /// 记录错误日志
        /// </summary> 
        /// <param name="message"></param>
        /// <param name="ex"></param>  
        public static void Error(string message, Exception ex = null)
        {
            var log = new LogEntry(LogLevel.Error, null, message, ex);
            WriteLog(log);
        }

        /// <summary>
        /// 记录崩溃日志
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>  
        /// <param name="eventId"></param>
        public static void Fatal(string title, Exception ex, string message = null, string eventId = null)
        {
            var log = new LogEntry(LogLevel.Fatal, title, message, ex, eventId);
            WriteLog(log);
        }

        /// <summary>
        /// 记录崩溃日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param> 
        /// <param name="eventId"></param>
        public static void Fatal(Exception ex, string message = null, string eventId = null)
        {
            var log = new LogEntry(LogLevel.Fatal, null, message, ex, eventId);
            WriteLog(log);
        }

        /// <summary>
        /// 记录崩溃日志
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param> 
        /// <param name="eventId"></param>
        public static void Fatal(string title, string message, string eventId = null)
        {
            var log = new LogEntry(LogLevel.Fatal, title, message, eventId: eventId);
            WriteLog(log);
        }


        /// <summary>
        /// 执行记录日志
        /// </summary>
        /// <param name="logEntry"></param>
        private static void WriteLog(LogEntry logEntry)
        {
            ALogger.Instance.Enqueue(logEntry);

            /*
            if (log.Level == LogLevel.None) return;
            //如果当前日志不能记日志则不记录
            if (!IsAllowLog(log.Level))
                return;
            try
            {
                StringBuilder msg = new StringBuilder($"[{(log.LoggerTime ?? DateTime.GetTimestamp).ToString(3)}]")
                      .Append(log.EventId.IsNullOrEmpty() ? " " : "[EventId]:" + log.EventId + " ")
                      .Append(log.Title.IsNullOrEmpty() ? null : log.Title + " ")
                      .AppendLine(log.Message.IsNullOrEmpty() ? null : log.Message);

                if (log.Exception != null)
                    msg.Append($"【")
                       .Append(log.Exception.GetType().Name)
                       .Append("】") //.Append("位置："+log.Exception.TargetSite?.DeclaringType.FullName)
                       .Append("详情：")
                       .AppendLine(log.Exception.GetExcetionMessage())
                       .Append("----------------------Exception End--------------------------");

                var path = FileHelper.GetFilePath(
                    Path.Combine(AppDomainHelper.GetRunRoot, "logs",
                         log.LoggerName.IsNullOrEmpty() ? log.Level.GetDescription().ToLower() : log.LoggerName),
                         (log.LoggerTime ?? DateTime.GetTimestamp).ToString(2) + ".log");
                
                await FileHelper.AppendWrittenFile(path, msg.ToString());
            }
            catch (Exception ex)
            {
                Trace.Fail(ex.Message, ex.GetExcetionMessage());
            }*/
        }


        /// <summary>
        /// 检查是否需要记录此等级的日志
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static bool IsAllowLog(LogLevel level)
        {
            return GetAllowLogLists().Contains(level);
        }
        public static IList<LogLevel> AllowLogLists = null;
        static readonly object _lockobj = new object();
        public static string Log_Level { get; set; }

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
                                    LogHelper.Fatal("日志等级配置有误", ex, logs, nameof(LogHelper));
                                }
                            }
                        }
                    }
                }
            }
            return AllowLogLists;
        }
        static void ClearTraceId()
        {
            // LogClient.Instance.ClearTraceId();
        }

        static void LogCompleted()
        {
            //_logger.Dispose();
        }
        static LogLevel ConvertLogLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug: return LogLevel.Debug;
                case LogLevel.Info: return LogLevel.Info;
                case LogLevel.Warn: return LogLevel.Warn;
                case LogLevel.Error: return LogLevel.Error;
                case LogLevel.Fatal: return LogLevel.Fatal;
                default: return LogLevel.Info;
            }
        }
    }
}
