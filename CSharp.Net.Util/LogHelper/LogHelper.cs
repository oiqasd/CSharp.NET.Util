using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 客户端写日志帮助文件
    /// </summary>
    public sealed class LogHelper
    {
        public static string Appid { get; set; }
        public static string Log_Level { get; set; }
        //static Logger _logger;
        static LogHelper()
        {
            // _logger = new Logger(Path.Combine(AppDomainHelper.GetRunRoot, "logs"));
        }

        /// <summary>
        /// 记录调试日志
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <param name="loggerName">文件名,默认用level</param>
        /// <param name="dateTime">default:now</param>
        /// <param name="eventId"></param>
        public static async Task Debug(string title, string message, Exception ex, string eventId = null, string loggerName = null, DateTime? dateTime = null)
        {
            ISystemLog log = new SystemLog(LogLevel.Debug, title, message, dateTime, loggerName, ex, eventId: eventId);
            await WriteLog(log);
        }

        /// <summary>
        /// 记录调试日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <param name="loggerName">文件名,默认用level</param>
        /// <param name="dateTime">default:now</param>
        /// <param name="eventId"></param>
        public static async Task Debug(string message, Exception ex = null, string eventId = null, string loggerName = null, DateTime? dateTime = null)
        {
            ISystemLog log = new SystemLog(LogLevel.Debug, string.Empty, message, dateTime, loggerName, ex, eventId: eventId);
            await WriteLog(log);
        }

        /// <summary>
        /// 记录调试日志
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="loggerName">文件名,默认用level</param>
        /// <param name="eventId"></param>
        public static async Task Debug(Exception ex, string eventId = null, string loggerName = null)
        {
            ISystemLog log = new SystemLog(LogLevel.Debug, null, null, null, loggerName, ex, eventId: eventId);
            await WriteLog(log);
        }

        /// <summary>
        /// 记录调试日志
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="loggerName">文件名,默认用level</param>
        /// <param name="dateTime">default:now</param>
        /// <param name="eventId"></param>
        public static async Task Debug(string title, string message, string eventId = null, string loggerName = null, DateTime? dateTime = null)
        {
            ISystemLog log = new SystemLog(LogLevel.Debug, title, message, dateTime, loggerName, eventId: eventId);
            await WriteLog(log);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static async Task Info(string message)
        {
            ISystemLog log = new SystemLog(LogLevel.Info, null, message, null, null);
            await WriteLog(log);
        }

        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        public static async Task Info(string title, string message)
        {
            ISystemLog log = new SystemLog(LogLevel.Info, title, message, null, null);
            await WriteLog(log);
        }

        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <param name="loggerName">文件名,默认用level</param>
        /// <param name="dateTime">default:now</param>
        public static async Task Info(string title, string message, Exception ex, string loggerName = null, DateTime? dateTime = null)
        {
            ISystemLog log = new SystemLog(LogLevel.Info, title, message, dateTime, loggerName, ex);
            await WriteLog(log);
        }

        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="loggerName">文件名,默认用level</param>
        /// <param name="dateTime">default:now</param>
        public static async Task Info(string title, string message, string loggerName, DateTime? dateTime = null)
        {
            ISystemLog log = new SystemLog(LogLevel.Info, title, message, dateTime, loggerName);
            await WriteLog(log);
        }

        /// <summary>
        /// 记录警告日志
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <param name="loggerName">文件名,默认用level</param>
        /// <param name="dateTime">default:now</param>
        public static async Task Warn(string title, string message, Exception ex, string loggerName = null, DateTime? dateTime = null)
        {
            ISystemLog log = new SystemLog(LogLevel.Warn, title, message, dateTime, loggerName, ex);
            await WriteLog(log);
        }
        /// <summary>
        /// 记录警告日志
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="loggerName">文件名,默认用level</param>
        /// <param name="dateTime">default:now</param>
        public static async Task Warn(string title, string message, string loggerName = null, DateTime? dateTime = null)
        {
            ISystemLog log = new SystemLog(LogLevel.Warn, title, message, dateTime, loggerName);
            await WriteLog(log);
        }
        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <param name="loggerName">文件名,默认用level</param>
        /// <param name="dateTime">default:now</param>
        public static async Task Error(string title, string message, Exception ex, string loggerName, DateTime? dateTime = null)
        {
            ISystemLog log = new SystemLog(LogLevel.Error, title, message, dateTime, loggerName, ex);
            await WriteLog(log);
        }
        public static async Task Error(string message, string loggerName = null, DateTime? dateTime = null)
        {
            ISystemLog log = new SystemLog(LogLevel.Error, null, message, dateTime, loggerName);
            await WriteLog(log);
        }

        public static async Task Error(Exception ex, string message = null, string loggerName = null, DateTime? dateTime = null)
        {
            ISystemLog log = new SystemLog(LogLevel.Error, null, message, dateTime, loggerName, ex);
            await WriteLog(log);
        }

        public static async Task Error(string title, string message, string loggerName, DateTime? dateTime = null)
        {
            ISystemLog log = new SystemLog(LogLevel.Error, title, message, dateTime, loggerName);
            await WriteLog(log);
        }
        /// <summary>
        /// 记录崩溃日志
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <param name="loggerName">文件名,默认用level</param>
        /// <param name="dateTime">默认当前</param>
        /// <param name="eventId"></param>
        public static async Task Fatal(string title, string message, Exception ex, string eventId = null, string loggerName = null, DateTime? dateTime = null)
        {
            ISystemLog log = new SystemLog(LogLevel.Fatal, title, message, dateTime, loggerName, ex, eventId);
            await WriteLog(log);
        }
        /// <summary>
        /// 记录崩溃日志
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <param name="loggerName">文件名,默认用level</param>
        /// <param name="dateTime"></param>
        /// <param name="eventId"></param>
        public static async Task Fatal(string title, Exception ex, string message = null, string eventId = null, string loggerName = null, DateTime? dateTime = null)
        {
            ISystemLog log = new SystemLog(LogLevel.Fatal, title, message, dateTime, loggerName, ex, eventId);
            await WriteLog(log);
        }

        /// <summary>
        /// 记录崩溃日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <param name="loggerName">文件名,默认用level</param>
        /// <param name="dateTime"></param>
        /// <param name="eventId"></param>
        public static async Task Fatal(Exception ex, string message = null, string eventId = null, string loggerName = null, DateTime? dateTime = null)
        {
            ISystemLog log = new SystemLog(LogLevel.Fatal, null, message, dateTime, loggerName, ex, eventId);
            await WriteLog(log);
        }

        /// <summary>
        /// 记录崩溃日志
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="loggerName">文件名,默认用level</param>
        /// <param name="dateTime"></param>
        /// <param name="eventId"></param>
        public static async Task Fatal(string title, string message, string eventId = null, string loggerName = null, DateTime? dateTime = null)
        {
            ISystemLog log = new SystemLog(LogLevel.Fatal, title, message, dateTime, loggerName, eventId: eventId);
            await WriteLog(log);
        }

        /// <summary>
        /// 执行记录日志
        /// </summary>
        /// <param name="log"></param>
        private static async Task WriteLog(ISystemLog log)
        {
            if (log.Level == LogLevel.None) return;
            log.AppId = Appid;
            //如果当前日志不能记日志则不记录
            if (!IsAllowLog(log.Level))
                return;
            try
            {
                StringBuilder msg = new StringBuilder($"[{(log.LoggerTime ?? DateTime.Now).ToString(3)}]")
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
                         (log.LoggerTime ?? DateTime.Now).ToString(2) + ".log");
                //await _logger.LogAsync(msg.ToString());
                await FileHelper.AppendWrittenFile(path, msg.ToString());
            }
            catch (Exception ex)
            {
                Trace.Fail(ex.Message, ex.GetExcetionMessage());
            }
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
        public static void ClearTraceId()
        {
            // LogClient.Instance.ClearTraceId();
        }

        public static void LogCompleted()
        {
            //_logger.Dispose();
        }
        public static LogLevel ConvertLogLevel(LogLevel level)
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
