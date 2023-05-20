using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp.Net.Util
{

    /// <summary>
    /// 日志等级枚举
    /// 0.None 不记录
    /// 1.DEBUG 指出细粒度信息事件对调试应用程序是非常有帮助的。 
    /// 2.INFO 表明消息在粗粒度级别上突出强调应用程序的运行过程。 
    /// 3.WARN 表明会出现潜在错误的情形。 
    /// 4.ERROR 指出虽然发生错误事件，但仍然不影响系统的继续运行。 
    /// 5.FATAL 指出每个严重的错误事件将会导致应用程序的退出。 
    /// </summary>
    public enum LogLevel
    {
        None = 0, Debug = 1, Info = 2, Warn = 3, Error = 4, Fatal = 5
    }

}
