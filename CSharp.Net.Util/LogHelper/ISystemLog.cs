using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 程序操作日志
    /// </summary>
    public interface ISystemLog
    {
        /// <summary>
        /// 事务ID
        /// </summary>
        string EventId { get; set; }
        string AppId { get; set; }
        // string Key { get; set; }
        string Title { get; set; }
        string Message { get; set; }
        Exception Exception { get; set; }
        /// <summary>
        /// 日志时间，同时用做日志文件名，默认当前
        /// </summary>
        DateTime? LoggerTime { get; set; }
        /// <summary>
        /// 文件夹名,默认用level
        /// </summary>
        string LoggerName { get; set; }

        /// <summary>
        /// 从进程开始到现在的时长，毫秒数
        /// </summary>
        long ElapsedTime { get; set; }

        /// <summary>
        /// 1 Debug 2 Info 3 Warn 4 Error 5 Fatal
        /// </summary>
        LogLevel Level { get; set; }

        string ToString();
    }
}
