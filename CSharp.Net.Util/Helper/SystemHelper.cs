using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace CSharp.Net.Standard.Util
{

    //待完善
    //1.获取主机运行资源 http://www.qb5200.com/article/475990.html


    /// <summary>
    /// 应用程序帮助类
    /// </summary>
    public class AppDomainHelper
    {
        /// <summary>
        /// 获取运行根目录
        /// </summary>
        public static string GetRunRoot
        {
            get
            {
                return System.AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        /// <summary>
        /// 自动调节工作线程数
        /// </summary>
        public static void GetThreadPoolStats(int initWork = 100, int initIO = 10, string environment = null)
        {
            var now = DateTime.Now;
            ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxIoThreads);
            ThreadPool.GetAvailableThreads(out int freeWorkerThreads, out int freeIoThreads);
            ThreadPool.GetMinThreads(out int minWorkerThreads, out int minIoThreads);

            int busyIoThreads = maxIoThreads - freeIoThreads;
            int busyWorkerThreads = maxWorkerThreads - freeWorkerThreads;

            if ((busyWorkerThreads >= (minWorkerThreads * 0.3M) && minWorkerThreads < maxWorkerThreads) || (busyIoThreads >= (minIoThreads * 0.3M) && minIoThreads < maxIoThreads))
            {
                int newWorkMin = busyWorkerThreads >= (minWorkerThreads * 0.3M) ? minWorkerThreads * 2 : minWorkerThreads;
                newWorkMin = newWorkMin > maxWorkerThreads * 0.8M ? (int)(maxWorkerThreads * 0.8M) : newWorkMin;

                int newIoMin = busyIoThreads >= (minIoThreads * 0.3M) ? minIoThreads * 2 : minIoThreads;
                newIoMin = newIoMin > maxIoThreads * 0.8M ? (int)(maxIoThreads * 0.8M) : newIoMin;

                newWorkMin = newWorkMin < initWork ? initWork : newWorkMin;
                newIoMin = newIoMin < initIO ? initIO : newIoMin;
                ThreadPool.SetMinThreads(newWorkMin, newIoMin);

                //#if DEBUG
                if (environment == "dev")
                {
                    Console.WriteLine($"{now.ToString(1)} ###worker:(Busy={busyWorkerThreads},Free={freeWorkerThreads},Min={newWorkMin},Max={maxWorkerThreads})");
                    Console.WriteLine($"{now.ToString(1)} ###iocp:(Busy={busyIoThreads},Free={freeIoThreads},Min={newIoMin},Max={maxIoThreads})");
                }
                //#endif
            }

            else if ((minWorkerThreads > initWork && busyWorkerThreads < (minWorkerThreads / 2)) || (minIoThreads > initIO && busyIoThreads < (minIoThreads / 2)))
            {
                int newWorkMin = (int)(minWorkerThreads * 0.7M);
                newWorkMin = newWorkMin < initWork ? initWork : newWorkMin;
                int newIoMin = (int)(minIoThreads * 0.6M);
                newIoMin = newIoMin < initIO ? initIO : newIoMin;
                ThreadPool.SetMinThreads(newWorkMin, newIoMin);
                if (environment == "dev")
                {
                    Console.WriteLine($"{now.ToString(1)} ###worker:(Busy={busyWorkerThreads},Free={freeWorkerThreads},Min={newWorkMin},Max={maxWorkerThreads})");
                    Console.WriteLine($"{now.ToString(1)} ###iocp:(Busy={busyIoThreads},Free={freeIoThreads},Min={newIoMin},Max={maxIoThreads})");
                }
                //#endif
            }
            else if (minIoThreads < initIO || minWorkerThreads < initWork)
            {
                ThreadPool.SetMinThreads(initWork, initIO);
                if (environment == "dev")
                {
                    Console.WriteLine($"{now.ToString(1)} ###Init MinThreads:{initWork},{initIO}");
                }
            }

        }

        /// <summary>
        /// 打印线程数
        /// </summary>
        /// <param name="iocp"></param>
        /// <param name="worker"></param>
        public static void PrintThreadPoolStats(out string iocp, out string worker)
        {
            ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxIoThreads);
            ThreadPool.GetAvailableThreads(out int freeWorkerThreads, out int freeIoThreads);
            ThreadPool.GetMinThreads(out int minWorkerThreads, out int minIoThreads);

            int busyIoThreads = maxIoThreads - freeIoThreads;
            int busyWorkerThreads = maxWorkerThreads - freeWorkerThreads;

            iocp = $"iocp:(Busy={busyIoThreads},Free={freeIoThreads},Min={minIoThreads},Max={maxIoThreads})";
            worker = $"worker:(Busy={busyWorkerThreads},Free={freeWorkerThreads},Min={minWorkerThreads},Max={maxWorkerThreads})";
        }

        /// <summary>
        /// 打印运行环境信息
        /// </summary>
        /// <returns>
        /// 192.168.9.92
        /// v2.4.2.2
        /// iocp:(Busy=0,Free=1000,Min=10,Max=1000)
        /// worker:(Busy=2,Free=32765,Min=100,Max=32767)
        /// 使用内存：689 mb
        /// GC:146mb
        /// </returns>
        public string PrintRunTimeInfo()
        {
            PrintThreadPoolStats(out string icop, out string worker);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(IpHelper.GetCurrentIP())
              .AppendLine($"v{GetVersion()}")
              .AppendLine(icop)
              .AppendLine(worker);
            var mem = GetMemory();
            sb.AppendLine($"使用内存：{mem / 1024 / 1024} mb");
            sb.AppendLine($"GC:{GC.GetTotalMemory(false) / 1024 / 1024}mb");
            //if (gc == 1)
            //GC.Collect(); 
            //if (gc == 2)
            //GC.Collect(GC.MaxGeneration);

            return sb.ToString();
        }
        /// <summary>
        /// 获取当前服务器IP
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentIP()
        {
            return IpHelper.GetCurrentIP();
        }

        /// <summary>
        /// 获取工作内存使用量bytes
        /// </summary>
        /// <returns></returns>
        public static long GetMemory()
        {
            return System.Diagnostics.Process.GetCurrentProcess().WorkingSet64;
        }

        /// <summary>
        /// 获取进程id
        /// </summary>
        /// <returns></returns>
        public static long GetProcessId()
        {
            return System.Diagnostics.Process.GetCurrentProcess().Id;
        }
        /// <summary>
        /// 获取程序集版本
        /// </summary>
        /// <returns></returns>
        public static Version GetVersion()
        {
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return version;
        }

        /// <summary>
        /// 获取程序集版本
        /// </summary>
        /// <returns></returns>
        public static string GetVersionStr()
        {
            Version version = GetVersion();
            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
        /// <summary>
        /// 获取.Net版本
        /// </summary>
        /// <returns></returns>
        public static string GetDotNetVersion()
        {
            return ExecuteCommand("dotnet --version");
        }

        /// <summary>
        /// 获取主机名
        /// </summary>
        /// <returns></returns>
        public static string GetHostName()
        {
            return ExecuteCommand("hostname");
        }

        /// <summary>
        /// SSH执行主机命令
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="rootUser"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public string ExecuteCommandSSH(string ip, string rootUser, string command)
        {
            var script = $"ssh -q -o \"StrictHostKeyChecking no\" -o \"UserKnownHostsFile=/dev/null\" -i /keys/{ip}/sshkey/id_rsa \"{rootUser}@{ip}\" \"{command}\"";
            return ExecuteCommand(script);
        }

        /// <summary>
        /// 执行主机命令
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        public static string ExecuteCommand(string script)
        {
            var escapedArgs = script.Replace("\"", "\\\"");
            var info = new ProcessStartInfo
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var isUnix = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            if (isUnix)
            {
                info.FileName = "/bin/bash";
                info.Arguments = $"-c \"{escapedArgs}\"";
            }
            else
            {
                info.FileName = "powershell";
                info.Arguments = escapedArgs;
            }

            var process = Process.Start(info);
            process.WaitForExit();
            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd();
                //output += process.StandardOutput.ReadToEnd();
                return output;
            }
            return null;
        }

        //在容器中操作宿主机:
        //1.生成 ssh key :ssh-keygen -t rsa -b 4096
        //2.把 public key 加入到 authorized_keys：cat /root/.ssh/id_rsa.pub > /root/.ssh/authorized_keys
        //3.启动容器并把 private key 挂载到容器中： docker run --name dotnet-interactive -d -v /root/.ssh/id_rsa:/root/.ssh/id_rsa -p 80:8888 dotnet-interactive:1.0.0
        //docker ps
        //4.安装 ssh client： #更新源:apt-get update -y   #安装:apt-get install openssh-client -y



    }

    public class SystemHelper
    {
        /// <summary>
        /// 获取用户数据缓存目录
        /// </summary>
        /// <returns></returns>
        public static string GetUserRoamingFolder(string path = "", bool defaultCreate = false)
        {
            string dir = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), path);
            if (defaultCreate)
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }

            return dir;
        }

    }
}