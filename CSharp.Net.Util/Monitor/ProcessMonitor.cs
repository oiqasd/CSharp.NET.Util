using System;
using System.Diagnostics;
using System.Linq;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 外部监控程序终止
    /// </summary>
    public class ProcessMonitor
    {
        /// <summary>
        /// 启动并监控退出
        /// </summary>
        /// <param name="appName">完整路径</param>
        public void ProcessSrartAndExitMonitor(string appName)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo(appName)
                {
                    UseShellExecute = false,
                    RedirectStandardError = true,
                }
            };
            process.Start();
            Console.WriteLine("等待程序退出");
            //等待程序退出
            process.WaitForExit();
            int exitCode = process.ExitCode;
            if (exitCode == 0)
            {
                Console.WriteLine("正常退出");
            }
            else if (exitCode == -1073741510)//0xC000013A
            {
                Console.WriteLine("程序被强制终止");
            }
            else
            {
                Console.WriteLine($"程序异常退出，退出码：{exitCode}");
            }
        }

        /// <summary>
        /// 监控外部程序退出
        /// </summary>
        /// <param name="appName">程序名称</param>
        public void ProcessExitMonitor(string appName)
        {
            var process = FindTargetProcess(appName);
            if (process == null)
            {
                Console.WriteLine("未找到目标进程");
                return;
            }

            Console.WriteLine($"等待程序退出,PID={process.Id},Name={process.ProcessName}");

            try
            {
                //process.WaitForExit();
                process.EnableRaisingEvents = true;
                //process.Exited += (sender, e) => { };
                process.Exited += OnProcessExited;

            }
            catch (Exception ex)
            {
                //实测WaitForExit()时通过任务管理器结束报：System.InvalidOperationException
                Console.WriteLine($"程序终止,{ex.GetType().FullName}:{ex.Message}");
            }
        }

        void OnProcessExited(object sender, EventArgs e)
        {
            var process = sender as Process;
            if (process == null)
            {
                Console.WriteLine($"进程已结束");
                return;
            }
            int exitCode = process.ExitCode;
            string exitReson = exitCode switch
            {
                0 => "正常退出",
                -1073741510 => "被强制终止",//0xC000013A，直接关闭程序
                _ => $"异常退出，退出码：{exitCode}" //事件订阅：通过任务管理器结束code=1
            };

            Console.WriteLine($"进程退出：{exitReson}");
            process.Dispose();
        }

        /// <summary>
        /// 查找系统中已运行的目标进程（取第一个匹配的进程）
        /// </summary>
        private static Process FindTargetProcess(string processName)
        {
            try
            {
                // 获取所有同名进程
                Process[] processes = Process.GetProcessesByName(processName);
                return processes.OrderByDescending(x=>x.StartTime)
                                .FirstOrDefault(); // 返回最新进程
            }
            catch (Exception ex)
            {
                Console.WriteLine($"查找进程失败：{ex.Message}");
                return null;
            }
        }
    }
}
