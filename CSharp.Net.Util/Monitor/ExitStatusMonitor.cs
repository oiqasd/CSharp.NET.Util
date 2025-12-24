using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 注册异常捕获事件（捕获未处理的异常）
    /// 适用程序内部
    /// </summary>
    public class ExitStatusMonitor
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "exit_log.txt");
        /// <summary>
        /// 注册全局异常捕捉器(捕捉崩溃级异常)
        /// </summary>
        public static void RegisterExceptionHandlers()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                string message = ex != null ? $"未处理异常:{ex.Message}(堆栈：{ex.StackTrace})" : "未知未处理异常";
                //系统级异常推出码
                ExitAbnormally(message, Marshal.GetHRForException(ex));
            };

            // 捕获UI线程异常（如果是WinForm/WPF程序需要添加）
            // Application.ThreadException += (sender, e) => { ... };
        }


        /// <summary>
        /// 正常退出处理
        /// </summary>
        /// <param name="reason"></param>
        /// <param name="exitCode"></param>
        public static void ExitNormally(string reason, int exitCode = 0)
        {
            LogExitStatus(exitCode, reason);
            Environment.Exit(exitCode);
        }

        private static void ExitAbnormally(string reason, int exitCode)
        {
            LogExitStatus(exitCode, reason);
            Environment.Exit(exitCode);
        }

        private static void LogExitStatus(int exitCode, string reason)
        {
            string log = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]" +
                         $"{(exitCode == 0 ? "正常退出" : "异常退出")} | " +
                         $"退出码：{exitCode} | " +
                         $"原因：{reason} \r\n";

            try
            {
                using (var fs = new FileStream(LogFilePath, FileMode.Append, FileAccess.Write, FileShare.Read))
                {
                    using (var sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(reason);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"日志记录失败：{ex.Message} 【{log}】");
            }
        }
    }
}
