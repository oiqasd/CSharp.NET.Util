using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CSharp.Net.Updater
{
    public class UpdateManager : IDisposable
    {
        private readonly string _appDir = AppDomain.CurrentDomain.BaseDirectory;
        private readonly string _updateTempDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "temps$0");// Path.Combine(Path.GetTempPath(), "temps$0");
        readonly string _updatePackageUrl = "https://your-update-server/api/updates.exe";
        readonly HttpClient _httpClient;
        string _updateInfo = string.Empty;
        string startApp = string.Empty;
        readonly AppType _AppType = AppType.Console;//1=windows应用程序 2=windows服务 3=linux服务

        UpdateManager() { _httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(60) }; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updatePackageUrl">订阅更新地址</param>
        /// <param name="AppType">1=windows应用程序 2=windows服务 3=linux服务</param>
        public UpdateManager(string updatePackageUrl, AppType appType = AppType.Console) : this(updatePackageUrl, "") { _AppType = appType; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updatePackageUrl"></param>
        /// <param name="update_info">则拼接<![CDATA[updatePackageUrl]]>地址，并优先判断该更新信息</param>
        public UpdateManager(string updatePackageUrl, string update_info) : this()
        {
            _updatePackageUrl = updatePackageUrl.TrimEnd('/');
            //if (!string.IsNullOrEmpty(update_info))
            _updateInfo = $"{_updatePackageUrl}/{update_info}".TrimEnd('/');
        }

        public async Task CheckForUpdate()
        {
            try
            {
                var hasUpdate = await CheckForUpdatesAsync();
                if (hasUpdate)
                {
                    Trace.WriteLine("发现新版本，正在准备更新...");
                    await ApplyUpdatesAsync();
                }
                else
                {
                    Trace.WriteLine($"当前已是最新版本。{GetCurrentVersion()}");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"检查更新失败: {ex.Message}");
            }
        }
        /// <summary>
        /// 检测是否需要更新
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckForUpdatesAsync()
        {
            try
            {
                var currentVersion = GetCurrentVersion();
                var latestVersion = await GetLatestVersionAsync();

                if (latestVersion == null) return false;
                return latestVersion > currentVersion;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"更新检查失败: {ex.Message}");
                return false;
            }
        }

        public string ComputeFileHash(string filePath)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    var hash = sha256.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
        /// <summary>
        /// 验证文件完整性
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="expectedHash"></param>
        /// <returns></returns>
        public bool VerifyFileIntegrity(string filePath, string expectedHash)
        {
            if (!File.Exists(filePath))
                return false;

            var actualHash = ComputeFileHash(filePath);
            return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
        }

        // 执行更新
        private async Task ApplyUpdatesAsync()
        {
            try
            {
                // 创建临时目录
                EnsureDirectoryAccess(_updateTempDir);

                // 下载更新包
                var updatePackagePath = await DownloadFileAsync(_updateInfo, _updateTempDir);

                // 解压更新文件
                if (updatePackagePath.Contains(".zip"))
                    ExtractUpdatePackage(updatePackagePath);

                // 应用更新（需要重启应用）
                RestartAppWithUpdates();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"更新应用失败: {ex.Message}");
                Cleanup();
            }
        }

        // 获取当前类库程序集版本
        //private Version GetCurrentVersion()
        //{
        //    return Assembly.GetExecutingAssembly().GetName().Version;
        //}

        /// <summary>
        /// 获取启动程序集版本
        /// </summary>
        /// <returns></returns>
        private Version GetCurrentVersion()
        {
            return Assembly.GetEntryAssembly().GetName().Version;
        }

        // 获取最新版本信息
        private async Task<Version> GetLatestVersionAsync()
        {
            try
            {
                // 检查URL是否有效
                if (!Uri.IsWellFormedUriString(_updateInfo, UriKind.Absolute))
                {
                    Trace.WriteLine($"更新检查失败：无效的URL - {_updateInfo}");
                    throw new InvalidDataException();
                }

                using (var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, _updateInfo)))
                {
                    response.EnsureSuccessStatusCode();

                    switch (response.Content.Headers.ContentType?.MediaType)
                    {
                        case "text/xml":
                            return await FromXml(_updateInfo);
                        case "application/x-zip-compressed":
                            break;
                        case "application/octet-stream":
                            return FromFile(response);
                        default:
                            throw new NotSupportedException("ContentType Error");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                throw ex;
            }
            catch (TaskCanceledException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            /*
            // 方法2：从文件名解析版本
            var fileName = Path.GetFileName(new Uri(_updatePackageUrl).LocalPath);
            var versionFromFileName = ParseVersionFromFileName(fileName);
            if (versionFromFileName != null)
            {
                return versionFromFileName;
            }*/

            // 


            //return new RemoteFileInfo
            //{
            //    ContentLength = response.Content.Headers.ContentLength,
            //    ContentType = response.Content.Headers.ContentType?.ToString(),
            //    LastModified = response.Content.Headers.LastModified
            //};
            //--
            //var response = await httpClient.GetStringAsync($"{_updateServerUrl}/version");
            //return new Version(response.Trim());


            throw new InvalidOperationException("无法获取最新版本信息");
        }

        /// <summary>
        /// xml
        /// </summary>
        /// <param name="updateInfo"></param>
        /// <returns></returns>
        async Task<Version> FromXml(string updateInfo)
        {
            var response = await _httpClient.GetStringAsync(updateInfo);
            var xml = XDocument.Parse(response);

            var app = from p in xml.Descendants("app")
                      select new
                      {
                          app = p.Attribute("id")?.Value??"",
                          version = p.Element("version")?.Value ?? "",
                          packageUrl = p.Element("package-url")?.Value ?? "",
                          startApp = p.Element("start-app")?.Value ?? "",
                          earliestStartHour = p.Element("es-h")?.Value,
                          whiteList = p.Element("white-list")?.Value,
                      };
            var ap = app.Where(x => AES.Decrypt(x.app) == Process.GetCurrentProcess().ProcessName).FirstOrDefault();
            if (ap == null) return null;
            if (!string.IsNullOrWhiteSpace(ap.earliestStartHour) && int.TryParse(ap.earliestStartHour, out int h) && h > DateTime.Now.Hour) return null;
            if (!string.IsNullOrWhiteSpace(ap.whiteList) && !ap.whiteList.Split(';').Contains("")) return null;
            _updateInfo = AES.Decrypt(ap.packageUrl);
            startApp = AES.Decrypt(ap.startApp);
            return new Version(ap.version);
        }

        /// <summary>
        /// 文件通过使用时间
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        Version FromFile(HttpResponseMessage response)
        {
            var lastModified = response.Content.Headers.LastModified;
            string path = Process.GetCurrentProcess().MainModule?.FileName;
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.LastWriteTime < lastModified)
            {
                return new Version(999, 999);
            }
            else
            {
                return new Version(0, 0);
            }
        }

        /// <summary>
        /// 从文件名解析版本
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private Version ParseVersionFromFileName(string fileName)
        {
            var regex = new Regex(@"v?(\d+\.\d+\.\d+(?:\.\d+)?)");
            var match = regex.Match(fileName);

            if (match.Success && Version.TryParse(match.Groups[1].Value, out var version))
            {
                return version;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="destinationPath"></param>
        /// <returns></returns>
        private async Task<string> DownloadFileAsync(string fileUrl, string destinationPath)
        {
            try
            {
                //using HttpClient httpClient = new HttpClient();
                using (var response = await _httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    string filePath = Path.Combine(destinationPath, GetFileNameFromUrl(fileUrl));

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                        {

                            var contentLength = response.Content.Headers.ContentLength;

                            if (contentLength.HasValue)
                            {
                                var totalBytes = contentLength.Value;
                                var buffer = new byte[8192];
                                long bytesRead = 0;
                                int bytes;

                                while ((bytes = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                {
                                    await fileStream.WriteAsync(buffer, 0, bytes);
                                    bytesRead += bytes;

                                    var progress = (int)((bytesRead * 100) / totalBytes);
                                    //Console.Write($"\r下载进度: {progress}%");
                                }
                            }
                            else
                            {
                                await stream.CopyToAsync(fileStream);
                            }
                            return filePath;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"下载文件失败: {ex.Message}");
                throw;
            }
        }
        /*
        // 下载更新包
        private async Task DownloadUpdatePackageAsyncSimple(string fileUrl)
        {
            var updatePackagePath = Path.Combine(_updateTempDir, "update.zip");

            using (var stream = await _httpClient.GetStreamAsync(fileUrl))
            using (var fileStream = new FileStream(updatePackagePath, FileMode.Create))
            {
                await stream.CopyToAsync(fileStream);
            }

            return updatePackagePath;

        }*/

        // 解压更新包
        private void ExtractUpdatePackage(string packagePath)
        {
            ZipFile.ExtractToDirectory(packagePath, _updateTempDir);//true
            File.Delete(packagePath);
        }

        /// <summary>
        /// 提取文件名
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string GetFileNameFromUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                // 获取URL路径中的文件名（自动忽略查询参数和锚点）
                return System.IO.Path.GetFileName(uri.LocalPath);
            }
            catch (UriFormatException ex)
            {
                Trace.WriteLine($"URL格式无效: {ex.Message}");
                return null;
            }
        }

        // 重启应用以应用更新
        private void RestartAppWithUpdates()
        {
            // if (OperatingSystem.IsWindows())
            string scriptPath = string.Empty;

            if (_AppType == AppType.Console)
                scriptPath = CreateUpdateApplicationScript();
            if (_AppType == AppType.WindowsService)
                scriptPath = CreateUpdateWindowServiceScript();

            //  else if (OperatingSystem.IsLinux())
            {
                //rsync -avz --exclude='*.txt' --exclude='/temp/' /源路径/ /目标路径/
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{scriptPath}\"",
                CreateNoWindow = true,
                UseShellExecute = true,
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Hidden
            });

            Trace.WriteLine("更新成功，程序正在重启。");
            // 延迟退出，确保批处理文件有机会执行
            Task.Delay(1000).ContinueWith(_ =>
            {
                Environment.Exit(0);
            });
            //Application.Exit();
            //Environment.Exit(0);
        }
        string CreateUpdateApplicationScript()
        {
            // 创建批处理脚本执行更新和重启
            string scriptFileName = $"update_{Guid.NewGuid():N}.bat";
            //scriptPath = Path.Combine(scriptPath, scriptFileName);
            var scriptPath = Path.Combine(_updateTempDir, scriptFileName);

            if (string.IsNullOrEmpty(startApp))
            {
                startApp = Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrWhiteSpace(startApp))
                {
                    startApp = Assembly.GetEntryAssembly().Location;
                }
                startApp = Path.GetFileName(startApp);
            }

            using (var writer = new StreamWriter(scriptPath))
            {
                writer.WriteLine("@echo off");
                writer.WriteLine($"timeout /t 3 /nobreak > nul");
                //writer.WriteLine($"xcopy /Y /E /I /S \"{_updateTempDir}\\*\" \"{_appDir}\" /EXCLUDE:\"{scriptPath}\"");
                writer.WriteLine($"robocopy {_updateTempDir} {_appDir} /E /XF {scriptFileName}");
                writer.WriteLine($"start {Path.Combine(_appDir, startApp)}");
                //延迟删除自身，确保批处理脚本执行完成
                //writer.WriteLine("ping 127.0.0.1 -n 2 -w 1000 > nul");
                writer.WriteLine($"timeout /t 5 /nobreak >nul");
                writer.WriteLine($"del /f /s /q \"%~dp0\\*\"");
                writer.WriteLine($"cd ..");
                writer.WriteLine($"rd /s /q \"{_updateTempDir}\" >nul 2>&1");
            }
            File.SetAttributes(scriptPath, FileAttributes.Normal);

            return scriptPath;
        }

        /// <summary>
        /// windows 服务脚本
        /// </summary>
        /// <param name="scriptPath"></param>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        string CreateUpdateWindowServiceScript(string serviceName = "")
        {
            string scriptFileName = $"update_{Guid.NewGuid():N}.bat";

            if (string.IsNullOrWhiteSpace(serviceName))
                serviceName = Process.GetCurrentProcess().ProcessName;

            if (string.IsNullOrEmpty(startApp))
            {
                startApp = Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrWhiteSpace(startApp))
                {
                    startApp = Assembly.GetEntryAssembly().Location;
                }
                startApp = Path.GetFileName(startApp);
            }

            //string startApp = Path.GetFileName(appPath);
            string scriptPath = Path.Combine(_updateTempDir, scriptFileName);
            string workPath = Path.Combine(_appDir, startApp);

            using (var writer = new StreamWriter(scriptPath))
            {
                writer.WriteLine("@echo off");
                writer.WriteLine($"timeout /t 3 /nobreak > nul");
                writer.WriteLine($"set workname={serviceName}");
                writer.WriteLine($"set workpath={workPath}");
                writer.WriteLine($"@sc stop %workname% >nul 2>&1");
                writer.WriteLine($"timeout /t 5 /nobreak >nul");
                //writer.WriteLine($"@sc delete %workname%");

                writer.WriteLine($"robocopy {_updateTempDir} {_appDir} /E /XF {scriptFileName}");
                writer.WriteLine("@sc start %workname%  >nul 2>&1");
                writer.WriteLine("REM @sc config %workname% start= AUTO");
                //延迟删除自身，确保批处理脚本执行完成
                writer.WriteLine("timeout /t 5 /nobreak >nul");
                writer.WriteLine($"cd ..");
                writer.WriteLine($"rd /s /q \"{_updateTempDir}\" >nul 2>&1");
                //writer.WriteLine($"del /f /s /q \"%~dp0\\*\"");
            }
            File.SetAttributes(scriptPath, FileAttributes.Normal);

            return scriptPath;
        }
        /// <summary>
        /// 备份恢复
        /// </summary>
        /// <param name="backupDir"></param>
        /// <param name="targetDir"></param>
        /// <returns></returns>
        private async Task RestoreFromBackupAsync(string backupDir, string targetDir)
        {
            if (!Directory.Exists(backupDir))
                return;

            Trace.WriteLine("正在从备份恢复...");

            var backupFiles = Directory.GetFiles(backupDir);
            foreach (var file in backupFiles)
            {
                var destFile = Path.Combine(targetDir, Path.GetFileName(file));
                try
                {
                    await Task.Run(() => File.Copy(file, destFile, true));
                    Trace.WriteLine("恢复文件: {File}", Path.GetFileName(file));
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("恢复文件失败: {File}", file + ex.Message);
                }
            }
        }

        private void EnsureDirectoryAccess(string directoryPath)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // 测试写入权限
                var testFilePath = Path.Combine(directoryPath, "test_permission.tmp");
                File.WriteAllText(testFilePath, "1");
                File.Delete(testFilePath);
            }
            catch (UnauthorizedAccessException)
            {
                throw new InvalidOperationException($"没有权限访问目录: {directoryPath}");
            }
        }

        // 清理临时文件
        private void Cleanup()
        {
            try
            {
                if (Directory.Exists(_updateTempDir))
                {
                    Directory.Delete(_updateTempDir, true);
                }
            }
            catch { /* 忽略清理错误 */ }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
