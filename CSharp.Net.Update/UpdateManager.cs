using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CSharp.Net.Update
{
    public class UpdateManager : IDisposable
    {
        private readonly string _appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private readonly string _updateTempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "temps$0");// Path.Combine(Path.GetTempPath(), "temps$0");
        readonly string _updatePackageUrl = "https://your-update-server/api/updates";
        readonly HttpClient _httpClient;
        string _updateInfo = string.Empty;
        string startApp = string.Empty;

        UpdateManager() { _httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(60) }; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="updatePackageUrl"></param>
        public UpdateManager(string updatePackageUrl) : this(updatePackageUrl, "") { }

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
                    Console.WriteLine("发现新版本，正在准备更新...");
                    await ApplyUpdatesAsync();
                }
                else
                {
                    Console.WriteLine("当前已是最新版本。");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"检查更新失败: {ex.Message}");
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
                Console.WriteLine($"更新检查失败: {ex.Message}");
                return false;
            }
        }

        // 执行更新
        private async Task ApplyUpdatesAsync()
        {
            try
            {
                // 创建临时目录
                EnsureDirectoryAccess(_updateTempPath);

                // 下载更新包
                var updatePackagePath = await DownloadFileAsync(_updateInfo, _updateTempPath);

                // 解压更新文件
                if (updatePackagePath.Contains(".zip"))
                    ExtractUpdatePackage(updatePackagePath);

                // 应用更新（需要重启应用）
                RestartAppWithUpdates();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新应用失败: {ex.Message}");
                Cleanup();
            }
        }

        // 获取当前应用版本
        //private Version GetCurrentVersion()
        //{
        //    return Assembly.GetExecutingAssembly().GetName().Version;
        //}

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
                    Console.WriteLine($"更新检查失败：无效的URL - {_updateInfo}");
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
                          app = p.Attribute("id").Value,
                          version = p.Element("version").Value,
                          packageUrl = p.Element("package-url").Value,
                          startApp = p.Element("start-app").Value,
                          earliestStartHour = p.Element("es-h").Value,
                          whiteList = p.Element("white-list").Value,
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
                                    Console.Write($"\r下载进度: {progress}%");
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
                Console.WriteLine($"下载文件失败: {ex.Message}");
                throw;
            }
        }
        /*
        // 下载更新包
        private async Task DownloadUpdatePackageAsyncSimple(string fileUrl)
        {
            var updatePackagePath = Path.Combine(_updateTempPath, "update.zip");

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
            ZipFile.ExtractToDirectory(packagePath, _updateTempPath);//true
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
                Console.WriteLine($"URL格式无效: {ex.Message}");
                return null;
            }
        }

        // 重启应用以应用更新
        private void RestartAppWithUpdates()
        {
            // 创建批处理脚本执行更新和重启
            var scriptPath = Path.Combine(_updateTempPath, "update_script.bat");

            if (string.IsNullOrEmpty(startApp))
            {
                startApp = Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrWhiteSpace(startApp))
                {
                    startApp = Assembly.GetEntryAssembly().Location;
                }

                startApp = Path.GetFileName(startApp);
            }
            // if (OperatingSystem.IsWindows())
            {
                using (var writer = new StreamWriter(scriptPath))
                {
                    writer.WriteLine("@echo off");
                    writer.WriteLine($"timeout /t 3 /nobreak > nul");
                    //writer.WriteLine($"xcopy /y /s \"{_updateTempPath}\\*\" \"{_appDirectory}\" /EXCLUDE:\"{scriptPath}\"");
                    writer.WriteLine($"robocopy {_updateTempPath} {_appDirectory} /E /XF update_script.bat");
                    writer.WriteLine($"start {Path.Combine(_appDirectory, startApp)}");
                    //延迟删除自身，确保批处理脚本执行完成
                    writer.WriteLine("ping 127.0.0.1 -n 2 -w 1000 > nul");
                    writer.WriteLine($"del /f /s /q \"%~dp0\\*\"");
                }
                File.SetAttributes(scriptPath, FileAttributes.Normal);
            }
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
                WindowStyle = ProcessWindowStyle.Hidden
            });
            //Application.Exit();
            Environment.Exit(0);
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
                if (Directory.Exists(_updateTempPath))
                {
                    Directory.Delete(_updateTempPath, true);
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
