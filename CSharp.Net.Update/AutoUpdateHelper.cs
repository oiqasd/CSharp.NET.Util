using System;
using System.Threading.Tasks;

namespace CSharp.Net.Update
{
    public class AutoUpdateHelper
    {
        // 上次检查更新的时间
        private DateTime _lastCheckTime = DateTime.MinValue;
        // 检查更新的时间间隔
        private readonly int _checkIntervalMinutes = 10;
        // 是否正在进行更新操作
        private bool _isUpdating = false;

        private string _subscribeUrl = string.Empty;

        /// <summary>
        /// 生成更新配置
        /// </summary>
        /// <param name="startApp">xx.exe</param>
        /// <param name="packageUrl">http://xxx/aa</param>
        /// <returns></returns>
        public string GetUpdateXml(string startApp, string packageUrl)
        {
            var xml = GenerateUtil.GetXmlInfo(startApp, packageUrl);
            //Console.WriteLine(xml);
            return xml;
        }

        // 启动自动更新检查器
        public void StartAutoUpdateChecker(string subscribeUrl)
        {
            this._subscribeUrl = subscribeUrl;
            // 创建定时器
            var timer = new System.Timers.Timer(1000 * 60 * 30); // 每30分钟检查一次
            //执行一次（false）还是一直执行(true)
            timer.AutoReset = true;
            //执行Elapsed事件
            timer.Elapsed += CheckForUpdates;
            timer.Enabled = true;
            timer.Start();
            // 程序启动时立即检查一次
            CheckForUpdates(null, null);
        }

        // 检查更新的方法
        private async void CheckForUpdates(object sender, System.Timers.ElapsedEventArgs e)
        {
            // 防止重复检查
            if (_isUpdating) return;

            try
            {
                _isUpdating = true;
                // 检查是否达到检查时间间隔
                if (new TimeSpan(DateTime.Now.Ticks - _lastCheckTime.Ticks).TotalMinutes < _checkIntervalMinutes)
                    return;

                _lastCheckTime = DateTime.Now;

                // 在后台线程检查更新，避免阻塞UI
                await Task.Run(() => PerformUpdateCheck());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"自动更新检查失败: {ex.Message}");
            }
            finally
            {
                _isUpdating = false;
            }
        }
        async Task PerformUpdateCheck()
        {
            using (UpdateManager updateManager = new UpdateManager(_subscribeUrl))
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
