using CSharp.Net.Util.CsHttp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 配置
    /// </summary>
    public class HttpClientSetting
    {
        /// <summary>
        /// 打印请求失败控制台日志
        /// </summary>
        public bool OutputErrorInConsole { get; set; } = false;
        /// <summary>
        /// 异常处理模式
        /// </summary>
        public ThrowExceptionMode ThrowExceptionMode { get; set; } = ThrowExceptionMode.Default;
        /// <summary>
        /// 日志记录级别
        /// Debug:将打印请求/响应日志
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Trace;
    }

    /// <summary>
    /// http请求工具类
    /// </summary>
    public sealed class HttpClientUtil
    {
        //private static readonly HttpClient _httpClient = null;
        //private static object _obj = new object();

        static CsHttpClientFactory _csHttpFactory { get; set; } = new CsHttpClientFactory();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configure"></param>
        /// <exception cref="ArgumentNullException">if configure is null</exception>
        public void Configure(Action<HttpClientSetting> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            HttpClientSetting setting = new HttpClientSetting();
            configure.Invoke(setting);

            OutputErrorInConsole = setting.OutputErrorInConsole;
            ThrowExceptionMode = setting.ThrowExceptionMode;
            LogLevel = setting.LogLevel;
        }

        /// <summary>
        /// 打印请求失败控制台日志
        /// </summary>
        static bool OutputErrorInConsole = false;
        /// <summary>
        /// 异常处理模式
        /// </summary>
        static ThrowExceptionMode ThrowExceptionMode = ThrowExceptionMode.Default;
        /// <summary>
        /// 日志记录级别
        /// </summary>
        static LogLevel LogLevel = LogLevel.Trace;

        /*   static HttpClientUtil()
         {
             if (_csHttpFactory == null)
                 lock (_obj)
                     if (_csHttpFactory == null)
                         _csHttpFactory = new CsHttpClientFactory();
         
             if (_httpClient == null)
                 lock (_obj)
                     if (_httpClient == null)
                     {
                         HttpClientHandler handler = new HttpClientHandler
                         {
                             UseDefaultCredentials = true,
                             SslProtocols = SslProtocols.None, // SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12,
                             ClientCertificateOptions = ClientCertificateOption.Automatic,
                             ServerCertificateCustomValidationCallback = (message, certificate, chain, sslPolicyErrors) =>
                            {
 #if DEBUG
                                //Console.WriteLine("\r\n\r\n==================message==================\r\n\r\n{0}", message.ToString());
                                //Console.WriteLine("\r\n\r\n==================cert=====================\r\n\r\n{0}", certificate.ToString());
                                //Console.WriteLine("\r\n\r\n==================chain====================\r\n\r\n{0}", chain.ToString());
                                //Console.WriteLine("\r\n\r\n==================chain status=============\r\n\r\n{0}", chain.ChainStatus.ToString());
                                //Console.WriteLine("\r\n\r\n==================errors===================\r\n\r\n{0}", sslPolicyErrors.ToString());
                                //Console.WriteLine("\r\n\r\n====================================================\r\n\r\n");
                                //Console.WriteLine($"Received certificate with subject {certificate.Subject}");
                                //Console.WriteLine("\r\n\r\n====================================================\r\n\r\n");
                                //Console.WriteLine($"Current policy errors: {sslPolicyErrors}");
                                //Console.WriteLine("\r\n\r\n====================================================\r\n\r\n");
 #endif
                                if (sslPolicyErrors == SslPolicyErrors.None)
                                    return true;
                                else
                                {
                                    if ((SslPolicyErrors.RemoteCertificateNameMismatch & sslPolicyErrors) == SslPolicyErrors.RemoteCertificateNameMismatch)
                                        Console.WriteLine("HttpClientUtil:cert name not match: {0}.", sslPolicyErrors);

                                    if ((SslPolicyErrors.RemoteCertificateChainErrors & sslPolicyErrors) == SslPolicyErrors.RemoteCertificateChainErrors)
                                    {
                                        foreach (X509ChainStatus status in chain.ChainStatus)
                                        {
                                            Console.WriteLine("HttpClientUtil:status code = {0}.", status.Status);
                                            Console.WriteLine("HttpClientUtil:Status info = {0}.", status.StatusInformation);
                                        }
                                    }
                                    Console.WriteLine("HttpClientUtil:cert check failed: {0}.", sslPolicyErrors);
                                }
                                return false;
                            }
                         };
                         _httpClient = new HttpClient(handler, false);
 #if NET6_0_OR_GREATER
                         //var socketsHttpHandler = new SocketsHttpHandler()
                         //{
                         //    ConnectTimeout = TimeSpan.FromSeconds(20),
                         //    PooledConnectionLifetime = TimeSpan.FromMinutes(5),//限制连接的生命周期,默认无限,缓解DNS解析问题
                         //    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),//空闲连接在连接池中的存活时间,<=NET5默认2min, >NET6 1min
                         //    MaxConnectionsPerServer = 200,//每个目标服务节点能建立的最大连接数 默认int.MaxValue
                         //};
                         //var httpClient = new HttpClient(socketsHttpHandler)
                         //{
                         //    Timeout = Timeout.InfiniteTimeSpan
                         //};
 #endif
                         _httpClient.DefaultRequestHeaders.ConnectionClose = true;//短链接
                         _httpClient.Timeout = Timeout.InfiniteTimeSpan;//禁用默认的超时设置
                     }
    } */

        public static async Task<string> PostAsync(string url, object data,
            Dictionary<string, string> headers = null,
           string encoding = "utf-8", int timeOutSecond = 60, 
           bool throwEx = true, bool connectionClose = false, string handleName = null)
        {

            string result = string.Empty;
            HttpContent httpContent = null;
            try
            {
                if (data != null)
                    httpContent = new StringContent(JsonHelper.Serialize(data), Encoding.GetEncoding(encoding), "application/json");

                PrintRequestLog("post", url, out string trackId, data);
                return await PostAsync(url, httpContent, trackId, headers, timeOutSecond, connectionClose, handleName: handleName);
            }
            catch (Exception ex)
            {
                OutputErrorLog(ex, url);
                if (IfThrow(throwEx))
                    throw;
            }
            finally
            {
                if (httpContent != null)
                {
                    httpContent.Dispose();
                    httpContent = null;
                }
            }
            return result;
        }
        /// <summary>
        /// Post请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="dataDic">参数</param>
        /// <param name="headers">header</param>
        /// <param name="httpContentType"></param>
        /// <param name="timeOutSecond">超时时间</param>
        /// <param name="throwEx">是否抛出异常</param>
        /// <param name="connectionClose">是否标记短链接</param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static async Task<string> PostAsync(string url, Dictionary<string, object> dataDic = null,
            HttpContentType httpContentType = HttpContentType.JSON, Dictionary<string, string> headers = null,
            string encoding = "utf-8", int timeOutSecond = 60, bool throwEx = true, bool connectionClose = false, string handleName = null)
        {
            string result = string.Empty;
            HttpContent httpContent = null;
            try
            {
                if ((httpContentType == HttpContentType.FormData) && dataDic != null)
                {
                    httpContent = ToFormUrlEncodedContent(dataDic);

                    //var fromData = new MultipartFormDataContent(Guid.NewGuid().ToString());
                    //foreach (var k in dataDic)
                    {
                        //if (k.Value.GetType() == typeof(String))
                        //if (!string.IsNullOrEmpty(k.Value))
                        //    fromData.Add(new StringContent(k.Value, Encoding.GetEncoding(encoding)), k.Key);
                        //formData.Add(new StringContent(k.Value), $"\"{k.Key}\""); //有些第三方要加""
                        //上传文件             
                        //List<FileStream>  fileList =new List<FileStream>();
                        //for (var i = 0; i < fileList.Count; i++)
                        //{
                        //    var file = fileList[i];
                        //    byte[] data;
                        //    using (var br = new BinaryReader(file))
                        //        data = br.ReadBytes((int)file.Length);
                        //    ByteArrayContent imageContent = new ByteArrayContent(data);
                        //    formData.Add(imageContent, "imgs", file.Name);
                        //}
                        //var boundary = formData.Headers.ContentType.Parameters.Single(p => p.Name == "boundary");
                        //boundary.Value = boundary.Value.Replace("\"", string.Empty);
                    }
                    //httpContent = fromData;
                }

                if ((httpContentType == HttpContentType.x_www_form_urlEncoded) && dataDic != null)
                    httpContent = ToFormUrlEncodedContent(dataDic);

                if (httpContentType == HttpContentType.JSON && dataDic != null)
                    httpContent = new StringContent(JsonHelper.Serialize(dataDic), Encoding.GetEncoding(encoding), "application/json");

                if (httpContentType == HttpContentType.QueryString && dataDic != null)
                {
                    StringBuilder sb = new StringBuilder();
                    dataDic.ForEach(x => { if (x.Value != null) sb.Append(x.Key).Append("=").Append(x.Value).Append("&"); });

                    if (!url.Contains("?")) url += "?";
                    if (url.Contains("&")) url += "&";

                    url = $"{url}{sb.ToString().TrimEnd('&')}";
                }
                PrintRequestLog("post", url, out string trackId, dataDic);
                return await PostAsync(url, httpContent, trackId, headers, timeOutSecond, connectionClose, handleName: handleName);

                /*
                PrintRequestLog("post", url, out string trackId, dataDic);
                //CancellationTokenSource cts = new CancellationTokenSource();
                //if (timeOutSecond > 0)
                //    cts.CancelAfter(timeOutSecond * 1000);
                var _httpClient = _csHttpFactory.CreateClient(connectionClose);
                if (timeOutSecond > 0)
                    _httpClient.Timeout = TimeSpan.FromSeconds(timeOutSecond);
                SetHeader(_httpClient, headers);
                using (HttpResponseMessage response = await _httpClient.PostAsync(url, httpContent))//, cts.Token
                {
                    response.EnsureSuccessStatusCode();
                    result = await response.Content.ReadAsStringAsync();
                }
                PrintResponseLog(trackId, result);*/
            }
            catch (Exception ex)
            {
                OutputErrorLog(ex, url);
                if (IfThrow(throwEx))
                    throw;
            }
            finally
            {
                if (httpContent != null)
                {
                    httpContent.Dispose();
                    httpContent = null;
                }
            }
            return result;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="file"></param>
        /// <param name="dataDic"></param>
        /// <param name="headers"></param>
        /// <param name="encoding"></param>
        /// <param name="timeOutSecond"></param>
        /// <param name="throwEx">是否抛出异常</param>
        /// <param name="connectionClose">是否标记短链接</param>
        /// <returns></returns>
        public static async Task<string> PostFileAsync(string url, PostFileDto file, Dictionary<string, object> dataDic = null,
                                                       Dictionary<string, string> headers = null, string encoding = "utf-8",
                                                       int timeOutSecond = -1, bool throwEx = true, bool connectionClose = false)
                 => await PostFileAsync(url, dataDic, headers, encoding, timeOutSecond, throwEx, connectionClose, file);

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="files"></param>
        /// <param name="dataDic"></param>
        /// <param name="headers"></param>
        /// <param name="encoding"></param>
        /// <param name="timeOutSecond"></param>
        /// <param name="throwEx">是否抛出异常</param>
        /// <param name="connectionClose">是否标记短链接</param>
        /// <returns></returns>
        public static async Task<string> PostFileAsync(string url, Dictionary<string, object> dataDic = null,
                                                       Dictionary<string, string> headers = null,
                                                       string encoding = "utf-8",
                                                       int timeOutSecond = -1,
                                                       bool throwEx = true,
                                                       bool connectionClose = false,
                                                       params PostFileDto[] files)
        {
            ByteArrayContent fileContent = null;
            try
            {
                using (var fromData = new MultipartFormDataContent())
                {
                    foreach (var f in files)
                    {
                        //int len = (int)f.Stream.Length;
                        //byte[] bt = new byte[len];
                        ////byte[] bt = ArrayPool<byte>.Shared.Rent(len);
                        //f.Stream.Read(bt, 0, len);
                        //var fileContent = new ByteArrayContent(bt);
                        if (f.Data.IsNullOrEmpty() && f.Stream != null)
                        {
                            using (var stream = new MemoryStream())
                            {
                                f.Stream.Position = 0;
                                await f.Stream.CopyToAsync(stream);

                                fileContent = new ByteArrayContent(stream.ToArray());
                            }
                        }
                        else
                        {
                            fileContent = new ByteArrayContent(f.Data);
                        }

                        fileContent.Headers.ContentType = new MediaTypeHeaderValue(f.ContentType);

                        //解决中文乱码
                        var hv = $"form-data;name=\"file\";filename=\"{f.FileName}\"";
                        byte[] bdis = Encoding.GetEncoding(encoding).GetBytes(hv);
                        hv = string.Empty;
                        foreach (byte b in bdis)
                            hv += (char)b;

                        fileContent.Headers.Add("Content-Disposition", hv);
                        fromData.Add(fileContent, f.Key ?? f.FileName, f.FileName);
                        //ArrayPool<byte>.Shared.Return(bt, true);
                    }

                    if (dataDic != null)
                    {
                        foreach (var d in dataDic)
                        {
                            fromData.Add(new StringContent(d.Value?.ToString(), Encoding.GetEncoding(encoding)), d.Key);
                        }
                    }
                    PrintRequestLog("post file", url, out string trackId, "file");
                    using HttpContent httpContent = fromData;
                    return await PostAsync(url, httpContent, trackId, headers, timeOutSecond, connectionClose);
                }
            }
            catch (Exception ex)
            {
                OutputErrorLog(ex, url);
                if (IfThrow(throwEx))
                    throw;
                return string.Empty;
            }
            finally
            {
                if (fileContent != null)
                {
                    fileContent.Dispose();
                    fileContent = null;
                }
            }
        }

        //public static async Task Download(string url)
        //{
        //    var stream = await _httpClient.GetStreamAsync(url);
        //    var count = 0;
        //    var buffer = ArrayPool<byte>.Shared.Rent(1024 * 1024);
        //    stream.Read(buffer, 0, buffer.Length);
        //    _tenants.Return(buffer);
        //}

        /// <summary>
        /// json提交
        /// </summary>
        /// <param name="url"></param>
        /// <param name="dataStr"></param>
        /// <param name="httpContentType"></param>
        /// <param name="headers"></param>
        /// <param name="encoding"></param>
        /// <param name="timeOutSecond"></param>
        /// <param name="throwEx">是否抛出异常</param>
        /// <param name="connectionClose">是否标记短链接</param>
        /// <returns></returns>
        public static async Task<string> PostAsync(string url, string dataStr,
            HttpContentType httpContentType = HttpContentType.JSON,
            Dictionary<string, string> headers = null, string encoding = "utf-8",
            int timeOutSecond = 60, bool throwEx = true, bool connectionClose = false, string handleName = null)
        {
            string result = string.Empty;
            HttpContent httpContent = null;
            try
            {
                if (dataStr.IsHasValue() && httpContentType != HttpContentType.QueryString)
                {
                    Encoding _encoding = Encoding.GetEncoding(encoding);
                    string mediaType = httpContentType == HttpContentType.JSON ? "application/json" : "application/x-www-form-urlencoded";
                    httpContent = new StringContent(dataStr, _encoding, mediaType);
                }
                if (dataStr.IsHasValue() && httpContentType == HttpContentType.QueryString)
                {
                    if (!url.Contains("?") && !dataStr.StartsWith("?")) url += "?";
                    url = url + dataStr;
                }

                PrintRequestLog("post", url, out string trackId, dataStr);
                return await PostAsync(url, httpContent, trackId, headers, timeOutSecond, connectionClose, handleName);
                /*
                //CancellationTokenSource cts = new CancellationTokenSource();
                //if (timeOutSecond > 0)
                //    cts.CancelAfter(timeOutSecond * 1000);
                var _httpClient = _csHttpFactory.CreateClient(connectionClose);
                if (timeOutSecond > 0)
                    _httpClient.Timeout = TimeSpan.FromSeconds(timeOutSecond);
                SetHeader(_httpClient, headers);
                using (HttpResponseMessage response = await _httpClient.PostAsync(url, httpContent))//, cts.Token
                {
                    response.EnsureSuccessStatusCode();
                    result = await response.Content.ReadAsStringAsync();
                }
                PrintResponseLog(trackId, result);
                */
            }
            catch (Exception ex)
            {
                OutputErrorLog(ex, url);
                if (IfThrow(throwEx))
                    throw;
                //ExceptionDispatchInfo.Capture(ex).Throw();
            }
            finally
            {
                if (httpContent != null)
                {
                    httpContent.Dispose();
                    httpContent = null;
                }
            }

            return result;
        }

        /// <summary>
        /// Get请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="prams"></param>
        /// <param name="timeOutSecond">default 5 second</param>
        /// <param name="retryNum">超时重试,默认0 不重试</param>
        /// <param name="throwEx">是否抛出异常</param>
        /// <param name="connectionClose">是否标记短链接</param>
        /// <returns></returns>
        public static async Task<string> GetAsync(string url,
            Dictionary<string, string> prams, Dictionary<string, string> headers = null,
            int timeOutSecond = 5, uint retryNum = 0, bool throwEx = true, bool connectionClose = false, string handleName = null)
        {
            StringBuilder sb = new StringBuilder();
            if (prams != null)
            {
                prams.ForEach(x => { if (!string.IsNullOrEmpty(x.Value)) sb.Append(x.Key).Append("=").Append(x.Value).Append("&"); });
            }
            return await GetAsync(url, sb.ToString(), headers, timeOutSecond, retryNum, throwEx, connectionClose, handleName: handleName);
        }

        /// <summary>
        /// http get
        /// </summary>
        /// <param name="url"></param>
        /// <param name="timeoutSeconds"></param>
        /// <param name="retryNum">超时重试,默认0 不重试</param>
        /// <param name="headers"></param>
        /// <param name="throwEx">是否抛出异常</param>
        /// <param name="connectionClose">是否标记短链接</param>
        /// <returns></returns>
        public static async Task<string> GetAsync(string url, int timeoutSeconds = 5, uint retryNum = 0, Dictionary<string, string> headers = null, bool throwEx = true, bool connectionClose = false, string handleName = null)
            => await GetAsync(url, null, headers, timeoutSeconds, retryNum, throwEx, connectionClose, handleName: handleName);

        /// <summary>
        /// Get请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="pramstr"></param>
        /// <param name="headers"></param>
        /// <param name="timeoutSeconds">default 5 second</param>
        /// <param name="retryNum">重试，默认0</param>
        /// <param name="throwEx">是否抛出异常</param>
        /// <param name="encoding">UTF-8</param>
        /// <param name="handleName"></param>
        /// <param name="connectionClose">是否标记短链接</param>
        /// <returns></returns>
        public static async Task<string> GetAsync(string url, string pramstr,
            Dictionary<string, string> headers = null,
            int timeoutSeconds = 5, uint retryNum = 0, bool throwEx = true,
            bool connectionClose = false, string encoding = "UTF-8", string handleName = null)
        {
            string result = string.Empty;

            if (pramstr.IsHasValue())
            {
                if (!url.Contains("?") && !pramstr.Contains("?")) url += "?";
                if (!url.Contains("?") && pramstr.Contains("?") && (!pramstr.StartsWith("?") || !pramstr.StartsWith("/"))) url += "/";
                if (url.Contains("&") && !pramstr.StartsWith("&")) url += "&";
                url = $"{url}{pramstr}";
            }

            Func<Task<string>> action = async () =>
            {
                PrintRequestLog("get", url, out string trackId);
                //CancellationTokenSource cts = new CancellationTokenSource();
                //cts.CancelAfter(timeOutSecond * 1000);

                var v = ValueStopwatch.StartNew();
                using (var _httpClient = await _csHttpFactory.CreateClient(timeoutSeconds, connectionClose, handleName))
                {
                    SetHeader(_httpClient, headers);

                    using (var response = await _httpClient.GetAsync(url))//,cts.Token
                    {
                        if (v.GetElapsedTime().TotalSeconds > 1)
                            LogHelper.Debug("HttpGet", "Response:" + v.GetElapsedTime().TotalMilliseconds + "ms " + url, eventId: trackId);

                        response.EnsureSuccessStatusCode();
                        var bytes = await response.Content.ReadAsByteArrayAsync();
                        result = Encoding.GetEncoding(encoding).GetString(bytes);
                    }
                }
                PrintResponseLog(trackId, result);
                return result;
            };

            result = await Invoke(action, url, throwEx);
            return result;
        }

        static async Task<string> PostAsync(string url, HttpContent httpContent, string trackId,
            Dictionary<string, string> headers = null, int timeoutSeconds = -1, bool connectionClose = false, string handleName = null)
        {
            var v = ValueStopwatch.StartNew();
            string result = null;
            var _httpClient = await _csHttpFactory.CreateClient(timeoutSeconds, connectionClose, handleName);

            SetHeader(_httpClient, headers);
            using (HttpResponseMessage response = await _httpClient.PostAsync(url, httpContent))
            {
                if (v.GetElapsedTime().TotalSeconds > 1)
                    LogHelper.Debug("HttpPost", "Response:" + v.GetElapsedTime().TotalMilliseconds + "ms " + url);

                response.EnsureSuccessStatusCode();
                result = await response.Content.ReadAsStringAsync();
            }
            PrintResponseLog(trackId, result);
            return result;
        }

        static async Task<string> Invoke(Func<Task<string>> action, string url, bool throwEx, uint retryNum = 0)
        {
            try
            {
                //var ret =  await action();
                var ret = await RetryUtil.Invoke(action, retryNum, exceptionTypes: typeof(TaskCanceledException));
                return ret;
            }
            catch (Exception ex)
            {
                //if (ex.GetType() == typeof(TaskCanceledException))
                //    ex = new TimeoutException();
                OutputErrorLog(ex, url, retryNum);
                if (IfThrow(throwEx)) throw;
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="httpMethod"></param>
        /// <param name="content"></param>
        /// <param name="timeout">秒 默认不超时</param>
        /// <param name="headers">请求头</param>
        /// <param name="connectionClose">是否短链接</param>
        /// <returns></returns>
        public static async Task<HttpResponseDto> SendAsync(string url,
            HttpMethod httpMethod, HttpContent content = null, int timeout = -1,
            Dictionary<string, string> headers = null, bool connectionClose = false, string handleName = null)
        {
            HttpResponseDto ret = new HttpResponseDto();
            var request = new HttpRequestMessage(httpMethod, url);
            //request.Content = new FormUrlEncodedContent(kv);
            //_httpClient.DefaultRequestHeaders.ConnectionClose = true;
            //_httpClient.Timeout = TimeSpan.FromSeconds(3);
            if (content != null)
                request.Content = content;
            var _httpClient = await _csHttpFactory.CreateClient(timeout, connectionClose, handleName);
            SetHeader(_httpClient, headers);

            PrintRequestLog(httpMethod.ToString(), url, out string trackId);
            var v = ValueStopwatch.StartNew();
            using (var response = await _httpClient.SendAsync(request))
            {
                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    ret.Data = res;
                    ret.Success = true;
                    PrintResponseLog(trackId, res);
                }
                else
                {
                    ret.Success = false;
                    PrintResponseLog(trackId, response.StatusCode.ToString());
                }
                ret.StatusCode = response.StatusCode.ToString();
            }
            request.Dispose();

            if (v.GetElapsedTime().TotalSeconds > 1)
                LogHelper.Debug("[SendAsync]", "Response:" + v.GetElapsedTime().TotalMilliseconds + "ms " + url, eventId: trackId);

            return ret;
        }

        /// <summary>
        /// 设置表头
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="headers"></param>
        private static void SetHeader(HttpClient httpClient, Dictionary<string, string> headers)
        {
            //httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //httpClient.DefaultRequestHeaders.Clear();
            //httpClient.DefaultRequestHeaders.ConnectionClose = true;

            if (headers.IsHasValue())
                headers.ForEach(x => httpClient.DefaultRequestHeaders.Add(x.Key, x.Value));

            //某些网站会检查User-Agent等header，缺失可能导致请求失败
            if (headers == null && httpClient.DefaultRequestHeaders.IsNullOrEmpty())
            {
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
                httpClient.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8");
                httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");//, br
                //_httpClient.DefaultRequestHeaders.Add("Referer", "https://www.xxx.com/");  
                //_httpClient.DefaultRequestHeaders.Add("Origin", "https://www.xxx.com/");
            }
        }

        private static bool IfThrow(bool throwEx)
        {
            return throwEx ||
                   ThrowExceptionMode == ThrowExceptionMode.Always ||
                   (ThrowExceptionMode == ThrowExceptionMode.Default && throwEx);
        }


        /// <summary>
        /// 打印日志
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="url"></param>
        /// <param name="retryNum"></param>
        private static void OutputErrorLog(Exception ex, string url, uint retryNum = 0)
        {
            if (OutputErrorInConsole)
                Console.WriteLine(DateTime.Now.ToString(1) + ex.Message + url);
            LogHelper.Fatal($"{nameof(HttpClientUtil)}", $"{ex.Message}{(retryNum > 0 ? $"RetryNum:{retryNum}" : "")}.{url}");

            if (ex is TaskCanceledException)
                ex = new AppTimeoutException("响应超时");
        }

        private static void PrintRequestLog(string method, string url, out string trackId, object data = null)
        {
            trackId = string.Empty;
            if (LogLevel != LogLevel.Debug) return;
            trackId = Guid.NewGuid().ToString("N");
            LogHelper.Debug($"[{nameof(HttpClientUtil)}]", $" {trackId},{method},{url},Request Data:{JsonHelper.Serialize(data)}");
        }

        private static void PrintResponseLog(string trackId, string data)
        {
            if (LogLevel == LogLevel.Debug)
                LogHelper.Debug($"[{nameof(HttpClientUtil)}]", $" {trackId},Response Data:{data}");
        }

        /// <summary>
        /// 文件下载
        /// </summary>
        /// <param name="downUrl"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async Task DownloadFileSimple(string downUrl, string filePath)
        {
            try
            {
                var httpClient = await _csHttpFactory.CreateClient();
                using var response = await httpClient.GetAsync(downUrl);
                using (var downloadedFileStream = File.Create(filePath))
                    await response.Content.CopyToAsync(downloadedFileStream);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"下载文件失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 文件下载
        /// </summary>
        /// <param name="downUrl"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async Task DownloadFileAsync(string downUrl, string filePath)
        {
            try
            {
                var _httpClient = await _csHttpFactory.CreateClient();
                using (var response = await _httpClient.GetAsync(downUrl, HttpCompletionOption.ResponseHeadersRead))
                {
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

        /// <summary>
        /// ToFormUrlEncodedContent
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="ignoreNullValues">忽略空对象</param>
        /// <param name="serializeComplexObjects">处理复杂对象</param>
        /// <returns></returns>
        static FormUrlEncodedContent ToFormUrlEncodedContent(Dictionary<string, object> dict,
            bool ignoreNullValues = true,
            bool serializeComplexObjects = false)
        {
            /*
            if (dict == null)
                throw new ArgumentNullException(nameof(dict));

            // 处理 null 值
            var keyValuePairs = ignoreNullValues
                ? dict.Where(kvp => kvp.Value != null)
                : dict.Where(kvp => kvp.Key != null);

            // 转换为字符串键值对
            var formData = keyValuePairs.Select(kvp =>
                new KeyValuePair<string, string>(
                    kvp.Key,
                    ConvertToString(kvp.Value)
                ));

            return new FormUrlEncodedContent(formData);
            */

            var keyValuePairs = new List<KeyValuePair<string, string>>();
            foreach (var kvp in dict)
            {
                if (ignoreNullValues && kvp.Value == null)
                    continue;

                string value = HandleValue(kvp.Value, serializeComplexObjects);
                keyValuePairs.Add(new KeyValuePair<string, string>(kvp.Key, value));
            }

            return new FormUrlEncodedContent(keyValuePairs);
        }

        private static string HandleValue(object value, bool serializeComplexObjects)
        {
            if (value == null)
                return string.Empty;

            // 处理基本类型
            if (value is string || value.GetType().IsValueType)
            {
                return value.ToString();
            }

            // 处理数组
            if (value is System.Collections.IEnumerable enumerable)
            {
                if (serializeComplexObjects)
                {
                    return JsonSerializer.Serialize(value);
                }
                else
                {
                    // 数组类型可能需要特殊处理，比如用逗号分隔
                    var list = enumerable.Cast<object>().Select(x => x?.ToString());
                    return string.Join(",", list);
                }
            }

            // 处理复杂对象
            if (serializeComplexObjects)
            {
                return JsonSerializer.Serialize(value);
            }

            return value.ToString();
        }
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    public class PostFileDto
    {
        /// <summary>
        /// 表单键
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// image/jpeg
        /// if (!new FileExtensionContentTypeProvider().Mappings.TryGetValue(Path.GetExtension(f.FileName), out var contenttype)) 
        /// throw new Exception("文件格式不存在");
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// 文件流
        /// </summary>
        public Stream Stream { get; set; }

        public byte[] Data { get; set; }
    }

    public class HttpResponseDto
    {
        /// <summary>
        /// 是否成功
        /// StatusCode:200-299
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// Success true:返回数据
        /// </summary>
        public string Data { get; set; }
        /// <summary>
        /// StatusCode
        /// </summary>
        public string StatusCode { get; set; }
    }

    /// <summary>
    /// 异常处理模式
    /// </summary>
    public enum ThrowExceptionMode
    {
        /// <summary>
        /// 模式，使用函数参数
        /// </summary>
        Default,
        /// <summary>
        /// 始终抛出异常
        /// </summary>
        Always,
        /// <summary>
        /// 从不抛出异常
        /// </summary>
        Never
    }

    /*
    ### IHttpClientFactory 示例

    1.添加 services.AddHttpClient();
    2.IHttpClientFactory _clientFactory=*
    3.
      var client = _clientFactory.CreateClient();
      //client.DefaultRequestHeaders.ConnectionClose = true;
      //client.Timeout = TimeSpan.FromSeconds(3);
      var request = new HttpRequestMessage(HttpMethod.Post, "url");
      //request.Content = new FormUrlEncodedContent(data);

      var response = await client.SendAsync(request);
      if (response.IsSuccessStatusCode)
      {
          var res = await response.Content.ReadAsStringAsync();
          //JObject obj = JObject.Parse(res);         
          return res;
      }
      else
      {
          return response.StatusCode.ToString();
      }
    */

}