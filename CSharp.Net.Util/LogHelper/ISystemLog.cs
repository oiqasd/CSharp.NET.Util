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
        DateTime? LoggerTime { get; set; }
        /// <summary>
        /// 文件名,默认用level
        /// </summary>
        string LoggerName { get; set; }
        /// <summary>
        /// 从进程开始到现在的时长，毫秒数
        /// </summary>
        long ElapsedTime { get; set; }
        /// <summary>
        /// 1 info 2Debug 3Error 4Final
        /// </summary>
        LogLevel Level { get; set; }

        string ToString();
    }
}
