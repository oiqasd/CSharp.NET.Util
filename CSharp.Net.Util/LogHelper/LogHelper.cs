using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

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
        /// <param name="beginTime"></param>
        public static void Debug(string title, string message, Exception ex, DateTime? beginTime = null)
        {
            ISystemLog log = new SystemLog();
            log.Title = title;
            log.Message = message;
            log.Level = LogLevel.Debug;
            log.Exception = ex.GetExcetionMessage();
            WriteLog(log, beginTime);
        }
        public static void Debug(string title, string message, DateTime? beginTime = null)
        {
            ISystemLog log = new SystemLog();
            log.Title = title;
            log.Message = message;
            log.Level = LogLevel.Debug;
            WriteLog(log, beginTime);
        }
        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <param name="beginTime"></param>
        public static void Info(string title, string message, Exception ex, DateTime? beginTime = null)
        {
            ISystemLog log = new SystemLog();
            log.Title = title;
            log.Message = message;
            log.Level = LogLevel.Info;
            log.Exception = ex.GetExcetionMessage();
            WriteLog(log, beginTime);
        }
        public static void Info(string title, string message, DateTime? beginTime = null)
        {
            ISystemLog log = new SystemLog();
            log.Title = title;
            log.Message = message;
            log.Level = LogLevel.Info;
            WriteLog(log, beginTime);
        }

        /// <summary>
        /// 记录警告日志
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public static void Warn(string title, string message, Exception ex, DateTime? beginTime = null)
        {
            ISystemLog log = new SystemLog();
            log.Title = title;
            log.Message = message;
            log.Level = LogLevel.Warn;
            log.Exception = ex.GetExcetionMessage();
            WriteLog(log, beginTime);
        }
        public static void Warn(string title, string message, DateTime? beginTime = null)
        {
            ISystemLog log = new SystemLog();
            log.Title = title;
            log.Message = message;
            log.Level = LogLevel.Warn;
            WriteLog(log, beginTime);
        }
        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public static void Error(string title, string message, Exception ex, DateTime? beginTime = null)
        {
            ISystemLog log = new SystemLog();
            log.Title = title;
            log.Message = message;
            log.Level = LogLevel.Error;
            log.Exception = ex.GetExcetionMessage();
            WriteLog(log, beginTime);
        }
        public static void Error(string title, string message, DateTime? beginTime = null)
        {
            ISystemLog log = new SystemLog();
            log.Title = title;
            log.Message = message;
            log.Level = LogLevel.Error;
            WriteLog(log, beginTime);
        }
        /// <summary>
        /// 记录崩溃日志
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <param name="beginTime"></param>
        public static void Fatal(string title, string message, Exception ex, DateTime? beginTime = null)
        {
            ISystemLog log = new SystemLog();
            log.Title = title;
            log.Message = message;
            log.Level = LogLevel.Fatal;
            log.Exception = ex.GetExcetionMessage();
            WriteLog(log, beginTime);
        }
        public static void Fatal(string title, string message, DateTime? beginTime = null)
        {
            ISystemLog log = new SystemLog();
            log.Title = title;
            log.Message = message;
            log.Level = LogLevel.Fatal;
            WriteLog(log, beginTime);
        }

        /// <summary>
        /// 执行记录日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="beginTime"></param>
        private static void WriteLog(ISystemLog log, DateTime? beginTime = null)
        {
            if (log.Level == LogLevel.None) return;
            log.AppId = Appid;
            //如果当前日志不能记日志则不记录
            if (!IsAllowLog(log.Level))
                return;
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine((beginTime ?? DateTime.Now).ToString(1));
                if (log.Title.IsNotNullOrEmpty())
                    sb.AppendLine(log.Title);
                if (log.Message.IsNotNullOrEmpty())
                    sb.AppendLine(log.Message);
                if (log.Exception.IsNotNullOrEmpty())
                    sb.AppendLine(log.Exception);

                // LogClient.Instance.Write(ConvertLogLevel(log.Level), log.AppId, "", "", "", msg, beginTime);
                var path = FileHelper.GetFilePath(Path.Combine(AppDomainHelper.GetRunRoot, "logs", log.Level.GetDescription().ToLower()), DateTime.Now.ToString(2) + ".log");
                FileHelper.AppendWrittenFile(path, sb.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
                                LogHelper.Error("日志等级配置有误", logs, ex);
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
