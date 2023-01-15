using System;
using System.Collections.Generic;
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
    /// <returns></returns>
    public static string GetExcetionMessage(this Exception ex)
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
