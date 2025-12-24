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
        //// 使用System.Diagnostics.StackTrace类解析文件名和行号
        //var stackTrace = new System.Diagnostics.StackTrace(ex, true);
        //var frame = stackTrace.GetFrame(0); // 获取最顶层的堆栈帧
        //string msg = ex.GetExcetionMessage() + (";FileName: " + frame.GetFileName());
        //msg += (";Line Number: " + frame.GetFileLineNumber());

        if (ex == null) return "";
        string exceMsg = "";
        exceMsg += ex.Message + "\n" + ex.StackTrace;
        if (ex.InnerException == null)
            return exceMsg;
        exceMsg += "\n" + ex.InnerException.GetExcetionMessage();
        return exceMsg;
    }
}
