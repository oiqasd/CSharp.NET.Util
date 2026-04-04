// ****************************************************
// * 创建日期：
// * 创建人：
// * 备注：
// ****************************************************

using System.Diagnostics;

namespace CSharp.Net.Util
{
    public class ALoggerTraceListener : TraceListener
    {
        public override void Write(string? message)
        {
            if (message.IsNullOrEmpty()) return;
            LogHelper.Log(LogLevel.Trace, message);
        }

        public override void WriteLine(string? message)
        {
            if (message.IsNullOrEmpty()) return;
            LogHelper.Log(LogLevel.Trace, message);
        }
    }
}
