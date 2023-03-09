using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

/// <summary>
/// 异常类扩展
/// </summary>
public static class ExceptionExtension
{
    /// <summary>
    /// 获取异常信息和堆栈信息
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="memberName">调用者方法名</param>
    /// <returns></returns>
    public static string GetExcetionMessage(this Exception ex, [CallerMemberName] string memberName = null)
    {
        string exceMsg = "";
        if (ex != null)
        {
            exceMsg += ex.Message + "\n" + ex.StackTrace;
            if (ex.InnerException != null)
            {
                exceMsg += "\n" + ex.InnerException.GetExcetionMessage();
            }
        }
        return exceMsg;
    }
}
