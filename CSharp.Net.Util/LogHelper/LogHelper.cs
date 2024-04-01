using System;
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
    public class LogHelper
    {
        public static string Appid { get; set; }
        public static string Log_Level { get; set; }

        /// <summary>
        /// 记录调试日志
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <param name="loggerName">文件名,默认用level</param>
        /// <param name="dateTime">default:now</param>
        public static async Task Debug(string title, string message, Exception ex, string loggerName = null, DateTime? dateTime = null)
        {
            ISystemLog log = new SystemLog(LogLevel.Debug, title, message, dateTime, loggerName, ex.GetExcetionMessage());
            await WriteLog(log);
        }

        /// <summary>
        /// 记录调试日志
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="loggerName">文件名,默认用level</param>
        /// <param name="dateTime">default:now</param>
        public static async Task Debug(string title, string message, string loggerName = null, DateTime? dateTime = null)
        {
            ISystemLog log = new SystemLog(LogLevel.Debug, title, message, dateTime, loggerName);
            await WriteLog(log);
        }
        public static async Task Info(string message, string loggerName = null, DateTime? dateTime = null)
        {
            ISystemLog log = new SystemLog(LogLevel.Info, null, message, dateTime, loggerName);
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
            ISystemLog log = new SystemLog(LogLevel.Info, title, message, dateTime, loggerName, ex.GetExcetionMessage());
            await WriteLog(log);
        }
        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="loggerName">文件名,默认用level</param>
        /// <param name="dateTime">default:now</param>
        public static async Task Info(string title, string message, string loggerName = null, DateTime? dateTime = null)
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
            ISystemLog log = new SystemLog(LogLevel.Warn, title, message, dateTime, loggerName, ex.GetExcetionMessage());
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
            ISystemLog log = new SystemLog(LogLevel.Error, title, message, dateTime, loggerName, ex.GetExcetionMessage());
            await WriteLog(log);
        }
        public static async Task Error(string message, string loggerName = null, DateTime? dateTime = null)
        {
            ISystemLog log = new SystemLog(LogLevel.Error, null, message, dateTime, loggerName);
            await WriteLog(log);
        }

        public static async Task Error(Exception ex, string message = null, string loggerName = null, DateTime? dateTime = null)
        {
            ISystemLog log = new SystemLog(LogLevel.Error, null, message, dateTime, loggerName, ex.GetExcetionMessage());
            await WriteLog(log);
        }

        public static async Task Error(string title, string message, string loggerName = null, DateTime? dateTime = null)
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
        /// <param name="dateTime"></param>
        public static async Task Fatal(string title, string message, Exception ex, string loggerName = null, DateTime? dateTime = null)
        {
            ISystemLog log = new SystemLog(LogLevel.Fatal, title, message, dateTime, loggerName, ex.GetExcetionMessage());

            await WriteLog(log);
        }

        /// <summary>
        /// 记录崩溃日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <param name="loggerName">文件名,默认用level</param>
        /// <param name="dateTime"></param>
        public static async Task Fatal(Exception ex, string loggerName = null, string message = null, DateTime? dateTime = null)
        {
            ISystemLog log = new SystemLog(LogLevel.Fatal, null, message, dateTime, loggerName, ex.GetExcetionMessage());

            await WriteLog(log);
        }

        /// <summary>
        /// 记录崩溃日志
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="loggerName">文件名,默认用level</param>
        /// <param name="dateTime"></param>
        public static async Task Fatal(string title, string message, string loggerName = null, DateTime? dateTime = null)
        {
            ISystemLog log = new SystemLog(LogLevel.Fatal, title, message, dateTime, loggerName);

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
                StringBuilder msg = new StringBuilder((log.LoggerTime ?? DateTime.Now).ToString(1))
                    .Append(string.IsNullOrEmpty(log.Title) ? "" : log.Title)
                    .Append(string.IsNullOrEmpty(log.Message) ? "" : log.Message)
                    .AppendLine(string.IsNullOrEmpty(log.Exception) ? "" : log.Exception);

                //LogClient.Instance.Write(ConvertLogLevel(log.Level), log.AppId, "", "", "", msg, dateTime);

                var path = FileHelper.GetFilePath(
                    Path.Combine(AppDomainHelper.GetRunRoot, "logs",
                         log.LoggerName.IsNullOrEmpty() ? log.Level.GetDescription().ToLower() : log.LoggerName),
                         (log.LoggerTime ?? DateTime.Now).ToString(2) + ".log");

                await FileHelper.AppendWrittenFile(path, msg.ToString());
            }
            catch (Exception ex)
            {
                Trace.Fail(ex.Message, ex.GetExcetionMessage());
                //Utils.WriteLog(log.ToString() + "\n " + exx.Message + exx.StackTrace, GetStrLogUrl());
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

        /// <summary>
        /// 通过配置文件设置允许记录的日志等级
        /// </summary>
        /// <returns></returns>
        public static IList<LogLevel> GetAllowLogLists()
        {
            if (AllowLogLists == null)
            {
                AllowLogLists = new List<LogLevel>();
                string logs = Log_Level?.Trim();
                if (logs.IsNullOrEmpty() || logs == "*")
                {
                    List<EnumItem> levels = EnumHelper.GetEnumItems(typeof(LogLevel));
                    foreach (var item in levels)
                    {
                        AllowLogLists.Add(EnumHelper.ParseByNameOrValue<LogLevel>(item.Name));
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
            return AllowLogLists;
        }
        public static void ClearTraceId()
        {
            // LogClient.Instance.ClearTraceId();
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
