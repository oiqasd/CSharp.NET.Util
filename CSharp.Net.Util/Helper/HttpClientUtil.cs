using CSharp.Net.Util.CsHttp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
        public bool PrintRequestErrorConsoleLog { get; set; } = false;
        /// <summary>
        /// 异常处理模式
        /// </summary>
        public ThrowExceptionMode ThrowExceptionMode { get; set; } = ThrowExceptionMode.Default;
        /// <summary>
        /// 日志记录级别
        /// Debug:将打印请求/响应日志
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.None;

        // internal HttpClientSetting()
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

            PrintRequestErrorConsoleLog = setting.PrintRequestErrorConsoleLog;
            ThrowExceptionMode = setting.ThrowExceptionMode;
            LogLevel = setting.LogLevel;
        }

        /// <summary>
        /// 打印请求失败控制台日志
        /// </summary>
        static bool PrintRequestErrorConsoleLog = false;
        /// <summary>
        /// 异常处理模式
        /// </summary>
        static ThrowExceptionMode ThrowExceptionMode = ThrowExceptionMode.Default;
        /// <summary>
        /// 日志记录级别
        /// </summary>
        static LogLevel LogLevel = LogLevel.None;

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
                         //    PooledConnectionLifetime = TimeSpan.FromMinutes(15),
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
        public static async Task<string> PostAsync(string url, Dictionary<string, string> dataDic = null,
            HttpContentType httpContentType = HttpContentType.JSON, Dictionary<string, string> headers = null,
            string encoding = "utf-8", int timeOutSecond = -1, bool throwEx = true, bool connectionClose = false)
        {
            string result = string.Empty;
            try
            {
                HttpContent httpContent = null;
                if ((httpContentType == HttpContentType.FormData) && dataDic != null)
                {
                    httpContent = new FormUrlEncodedContent(dataDic);

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
                    httpContent = new FormUrlEncodedContent(dataDic);

                if (httpContentType == HttpContentType.JSON && dataDic != null)
                    httpContent = new StringContent(JsonHelper.Serialize(dataDic), Encoding.GetEncoding(encoding), "application/json");

                if (httpContentType == HttpContentType.QueryString && dataDic != null)
                {
                    StringBuilder sb = new StringBuilder();
                    dataDic.ForEach(x => { if (!string.IsNullOrEmpty(x.Value)) sb.Append(x.Key).Append("=").Append(x.Value).Append("&"); });

                    if (!url.Contains("?")) url += "?";
                    if (url.Contains("&")) url += "&";

                    url = $"{url}{sb.ToString().TrimEnd('&')}";
                }
                PrintRequestLog("post", url, out string trackId, dataDic);
                return await PostAsync(url, httpContent, trackId, headers, timeOutSecond, connectionClose);

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
                DoPrintRequestErrorConsoleLog(ex, url, throwEx);
            }
            return result;
        }

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
        public static async Task<string> PostFileAsync(string url, List<PostFileDto> files, Dictionary<string, object> dataDic = null, Dictionary<string, string> headers = null,
            string encoding = "utf-8", int timeOutSecond = -1, bool throwEx = true, bool connectionClose = false)
        {
            string result = string.Empty;
            try
            {
                using (var fromData = new MultipartFormDataContent())
                {
                    foreach (var f in files)
                    {
                        int len = (int)f.Stream.Length;
                        byte[] bt = new byte[len];
                        //byte[] bt = ArrayPool<byte>.Shared.Rent(len);
                        f.Stream.Read(bt, 0, len);

                        var fileContent = new ByteArrayContent(bt);
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue(f.ContentType);

                        //解决中文乱码
                        var hv = $"form-data; name=\"{f.Key ?? f.FileName}\"; filename=\"{f.FileName}\"";
                        byte[] bdis = Encoding.GetEncoding(encoding).GetBytes(hv);
                        hv = "";
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
                    HttpContent httpContent = fromData;
                    return await PostAsync(url, httpContent, trackId, headers, timeOutSecond, connectionClose);
                    /*
                     *
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
            }
            catch (Exception ex)
            {
                DoPrintRequestErrorConsoleLog(ex, url, throwEx);
            }

            return result;
        }

        //public static async Task Download(string url)
        //{
        //    var stream = await _httpClient.GetStreamAsync(url);
        //    var count = 0;
        //    var buffer = ArrayPool<byte>.Shared.Rent(1024 * 1024);
        //    stream.Read(buffer, 0, buffer.Length);
        //    pool.Return(buffer);
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
            HttpContentType httpContentType = HttpContentType.JSON, Dictionary<string, string> headers = null,
            string encoding = "utf-8", int timeOutSecond = -1, bool throwEx = true, bool connectionClose = false)
        {
            string result = string.Empty;
            try
            {
                HttpContent httpContent = null;
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
                return await PostAsync(url, httpContent, trackId, headers, timeOutSecond, connectionClose);
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
                DoPrintRequestErrorConsoleLog(ex, url, throwEx);
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
        /// <param name="throwEx">是否抛出异常</param>
        /// <param name="connectionClose">是否标记短链接</param>
        /// <returns></returns>
        public static async Task<string> GetAsync(string url, Dictionary<string, string> prams, Dictionary<string, string> headers = null, int timeOutSecond = 5, bool throwEx = true, bool connectionClose = false)
        {
            StringBuilder sb = new StringBuilder();
            if (prams != null)
            {
                prams.ForEach(x => { if (!string.IsNullOrEmpty(x.Value)) sb.Append(x.Key).Append("=").Append(x.Value).Append("&"); });
            }
            return await GetAsync(url, sb.ToString(), headers, timeOutSecond, throwEx, connectionClose);
        }
        /// <summary>
        /// http get
        /// </summary>
        /// <param name="url"></param>
        /// <param name="pramstr"></param>
        /// <param name="timeOutSecond">default 5 second</param>
        /// <param name="throwEx">是否抛出异常</param>
        /// <param name="connectionClose">是否标记短链接</param>
        /// <returns></returns>
        public static async Task<string> GetAsync(string url, string pramstr, int timeOutSecond = 5, bool throwEx = true, bool connectionClose = false) => await GetAsync(url, pramstr, headers: null, timeOutSecond, throwEx, connectionClose);

        /// <summary>
        /// http get
        /// </summary>
        /// <param name="url"></param>
        /// <param name="timeOutSecond"></param>
        /// <param name="throwEx">是否抛出异常</param>
        /// <param name="connectionClose">是否标记短链接</param>
        /// <returns></returns>
        public static async Task<string> GetAsync(string url, int timeOutSecond, bool throwEx = true, bool connectionClose = false) => await GetAsync(url, null, timeOutSecond, throwEx, connectionClose);


        static async Task<string> PostAsync(string url, HttpContent httpContent, string trackId, Dictionary<string, string> headers = null, int timeOutSecond = -1, bool connectionClose = false)
        {
            var v = ValueStopwatch.StartNew();
            string result = null;
            var _httpClient = _csHttpFactory.CreateClient(connectionClose);

            if (timeOutSecond > 0)
                _httpClient.Timeout = TimeSpan.FromSeconds(timeOutSecond);
            SetHeader(_httpClient, headers);
            using (HttpResponseMessage response = await _httpClient.PostAsync(url, httpContent))
            {
                if (v.GetElapsedTime().TotalSeconds > 1)
                    LogHelper.Debug("[HttpPost]", "Response:" + v.GetElapsedTime().TotalMilliseconds + "ms " + url);

                response.EnsureSuccessStatusCode();
                result = await response.Content.ReadAsStringAsync();
            }
            PrintResponseLog(trackId, result);
            return result;
        }

        /// <summary>
        /// Get请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="pramstr"></param>
        /// <param name="headers"></param>
        /// <param name="timeOutSecond">default 5 second</param>
        /// <param name="throwEx">是否抛出异常</param>
        /// <param name="connectionClose">是否标记短链接</param>
        /// <returns></returns>
        public static async Task<string> GetAsync(string url, string pramstr = null, Dictionary<string, string> headers = null, int timeOutSecond = 5, bool throwEx = true, bool connectionClose = false)
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
                var _httpClient = _csHttpFactory.CreateClient(connectionClose);

                if (timeOutSecond > 0)
                    _httpClient.Timeout = TimeSpan.FromSeconds(timeOutSecond);
                SetHeader(_httpClient, headers);
                using (var response = await _httpClient.GetAsync(url))//,cts.Token
                {
                    if (v.GetElapsedTime().TotalSeconds > 1)
                        LogHelper.Debug("[HttpGet]", "Response:" + v.GetElapsedTime().TotalMilliseconds + "ms " + url, eventId: trackId);

                    response.EnsureSuccessStatusCode();
                    result = await response.Content.ReadAsStringAsync();
                }

                PrintResponseLog(trackId, result);
                return result;
            };

            result = await Invoke(action, url, throwEx);
            return result;
        }

        static async Task<string> Invoke(Func<Task<string>> action, string url, bool throwEx)
        {
            try
            {
                var ret = await action();
                return ret;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.GetType() == typeof(TaskCanceledException))
                    ex = new TimeoutException();

                if (PrintRequestErrorConsoleLog)
                    Console.WriteLine(DateTime.Now.ToString(1) + ex.Message + url);

                LogHelper.Fatal("HttpClientUtil", msg + " " + url, ex);

                if (ThrowExceptionMode == ThrowExceptionMode.Default && throwEx) throw;
                if (ThrowExceptionMode == ThrowExceptionMode.Always) throw;
                return null;
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="httpMethod"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static async Task<HttpResponseDto> SendAsync(string url, HttpMethod httpMethod, HttpContent content = null, bool connectionClose = false)
        {
            PrintRequestLog("send", url, out string trackId);
            HttpResponseDto ret = new HttpResponseDto();
            var request = new HttpRequestMessage(httpMethod, url);
            //request.Content = new FormUrlEncodedContent(kv);
            //_httpClient.DefaultRequestHeaders.ConnectionClose = true;
            //_httpClient.Timeout = TimeSpan.FromSeconds(3);
            if (content != null)
                request.Content = content;
            var _httpClient = _csHttpFactory.CreateClient(connectionClose);
            var response = await _httpClient.SendAsync(request);
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
            return ret;
        }

        /// <summary>
        /// 设置表头
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="headers"></param>
        private static void SetHeader(HttpClient httpClient, Dictionary<string, string> headers)
        {
            //_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //httpClient.DefaultRequestHeaders.Clear();
            //httpClient.DefaultRequestHeaders.ConnectionClose = true;
            if (headers.IsHasValue())
                headers.ForEach(x => httpClient.DefaultRequestHeaders.Add(x.Key, x.Value));
        }

        /// <summary>
        /// 打印日志
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="url"></param>
        /// <param name="throwEx"></param>
        private static void DoPrintRequestErrorConsoleLog(Exception ex, string url, bool throwEx)
        {
            string msg = DateTime.Now.ToString(1) + ex.Message + url;
            if (PrintRequestErrorConsoleLog)
                Console.WriteLine(msg);

            LogHelper.Fatal($"[{nameof(HttpClientUtil)}]", url, ex);

            if (ThrowExceptionMode == ThrowExceptionMode.Default && throwEx) throw ex;
            if (ThrowExceptionMode == ThrowExceptionMode.Always) throw ex;
        }

        private static void PrintRequestLog(string method, string url, out string trackId, object data = null)
        {
            trackId = string.Empty;
            if (LogLevel == LogLevel.None || LogLevel >= LogLevel.Info) return;
            trackId = Guid.NewGuid().ToString("N");
            LogHelper.Debug($"[{nameof(HttpClientUtil)}]", $" {trackId},{method},{url},Request Data:{JsonHelper.Serialize(data)}");
        }

        private static void PrintResponseLog(string trackId, string data)
        {
            if (LogLevel == LogLevel.None || LogLevel >= LogLevel.Info) return;
            LogHelper.Debug($"[{nameof(HttpClientUtil)}]", $" {trackId},Response Data:{data}");
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