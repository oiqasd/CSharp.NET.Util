using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp.Net.Util.Log
{
    /// <summary>
    /// 程序操作日志
    /// </summary>
    public interface ISystemLog
    {
        /// <summary>
        /// 事务ID
        /// </summary>
        string TransferId { get; set; }
        string AppId { get; set; }
        // string Key { get; set; }
        string Title { get; set; }
        string Message { get; set; }
        string Exception { get; set; }
        DateTime CreateTime { get; set; }

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
