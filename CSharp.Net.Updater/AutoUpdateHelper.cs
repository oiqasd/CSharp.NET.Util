using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.Net.Updater
{
    /// <summary>
    /// 自动更新组件，默认每30分钟检查一次，程序启动后30秒开始第一次检查
    /// </summary>
    public class AutoUpdateHelper
    {
        // 上次检查更新的时间
        //private DateTime _lastCheckTime = DateTime.MinValue;

        // 是否正在进行更新操作
        private bool _isUpdating = false;

        private string _subscribeUrl = string.Empty;
        private Timer _updateTimer;
        private AppType _appType;
        /// <summary>
        /// 生成更新配置
        /// </summary>
        /// <param name="startApp">启动文件名：xx.exe</param>
        /// <param name="packageUrl">包下载地址：http://xxx/aa.exe</param>
        /// <returns></returns>
        public string GetUpdateXml(string startApp, string packageUrl)
        {
            if (string.IsNullOrWhiteSpace(startApp))
                startApp = Assembly.GetEntryAssembly()?.GetName().Name + ".exe";

            var xml = GenerateUtil.GetXmlInfo(startApp, packageUrl);

            return xml;
        }

        /// <summary>
        /// 启动自动更新检查器
        /// </summary>
        /// <param name="subscribeUrl">订阅地址：http://xxxx/AutoUpdater.xml</param>
        /// <param name="appType">应用类型</param>
        /// <param name="dueTime">启动后多少秒进行更新检查，0：立即</param>
        /// <param name="interval">检查频率(秒)，默认30分钟</param>
        public void StartAutoUpdateChecker(
            string subscribeUrl, 
            AppType appType = AppType.Console,
            int dueTime=30,
            int interval=30*60)
        {
            try
            {
                _appType = appType;
                _subscribeUrl = subscribeUrl;

                // 设置定时器，按配置间隔检查更新
                _updateTimer = new Timer(async _ =>
                {
                    await CheckForUpdates();
                }, null, dueTime * 1000, 1000 * interval);

                //// 创建定时器
                //var timer = new System.Timers.Timer(1000 * 60 * 30); // 每30分钟检查一次 
                //timer.AutoReset = true;//执行一次（false）还是一直执行(true) 
                //timer.Elapsed += CheckForUpdates;//执行Elapsed事件
                //timer.Enabled = true;
                //timer.Start();
                //// 程序启动时立即检查一次
                //CheckForUpdates(null, null);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"启动自动更新检查器失败: {ex.Message}");
            }
        }

        // 检查更新的方法
        private async Task CheckForUpdates()
        {
            // 防止重复检查
            if (_isUpdating) return;

            try
            {
                _isUpdating = true;
                // 检查是否达到检查时间间隔
                //if (new TimeSpan(DateTime.Now.Ticks - _lastCheckTime.Ticks).TotalMinutes < _checkIntervalMinutes)
                //    return;
                //_lastCheckTime = DateTime.Now;

                // 在后台线程检查更新，避免阻塞UI
                await Task.Run(() => PerformUpdateCheck());
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"自动更新检查失败: {ex.Message}");
            }
            finally
            {
                _isUpdating = false;
            }
        }
        async Task PerformUpdateCheck()
        {
            using (UpdateManager updateManager = new UpdateManager(_subscribeUrl, _appType))
            {
                await updateManager.CheckForUpdate();
            }
        }


        // 检查用户是否活跃
        private bool IsUserActive()
        {
            // 实际实现中应该检测用户输入活动
            // 这里简化为返回false
            return false;
        }


        //// 通知用户有更新可用
        //private void NotifyUserUpdateAvailable(VersionInfo versionInfo)
        //{
        //    // 在UI线程上显示更新提示
        //    this.Invoke((MethodInvoker)delegate
        //    {
        //        if (MessageBox.Show($"发现新版本 {versionInfo.Version}。是否立即更新？",
        //            "更新可用", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        //        {
        //            if (DownloadUpdate(versionInfo.DownloadUrl))
        //            {
        //                InstallUpdate();
        //            }
        //        }
        //    });
        //}


        /*
        // 版本信息类
        public class VersionInfo
        {
            public Version Version { get; set; }
            public string DownloadUrl { get; set; }
            public string ReleaseNotes { get; set; }
        }*/
    }
}
