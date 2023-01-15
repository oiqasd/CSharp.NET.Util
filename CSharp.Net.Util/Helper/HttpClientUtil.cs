using CSharp.Net.Standard.Util.NewtJson;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace CSharp.Net.Standard.Util
{
    /// <summary>
    /// http请求工具类
    /// </summary>
    public class HttpClientUtil
    {
        private static readonly HttpClient _httpClient = null;
        static object _obj = new object();
        static HttpClientUtil()
        {
            if (_httpClient == null)
                lock (_obj)
                    if (_httpClient == null)
                    {
                        HttpClientHandler handler = new HttpClientHandler
                        {
                            UseDefaultCredentials = true,
                            SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12,
                            ClientCertificateOptions = ClientCertificateOption.Automatic,
                            ServerCertificateCustomValidationCallback = (message, certificate, chain, sslPolicyErrors) =>
                           {
#if DEBUG
                               Console.WriteLine("\r\n\r\n==================message==================\r\n\r\n{0}", message.ToString());
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
                                   {
                                       Console.WriteLine("HttpClientUtil:cert name not match: {0}.", sslPolicyErrors);
                                   }

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

                        _httpClient = new HttpClient(handler);
#if NET6
                        //var socketsHttpHandler = new SocketsHttpHandler()
                        //{
                        //    ConnectTimeout = TimeSpan.FromSeconds(20),
                        //};
                        //var httpClient = new HttpClient(socketsHttpHandler)
                        //{
                        //    Timeout = Timeout.InfiniteTimeSpan
                        //};
#endif
                        _httpClient.DefaultRequestHeaders.ConnectionClose = true;//短链接
                        _httpClient.Timeout = Timeout.InfiniteTimeSpan;//禁用默认的超时设置

                    }
        }

        /// <summary>
        /// Post请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="dataDic">参数</param>
        /// <param name="headers">header</param>
        /// <param name="httpContentType"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static async Task<string> PostAsync(string url, Dictionary<string, string> dataDic = null, HttpContentType httpContentType = HttpContentType.JSON, Dictionary<string, string> headers = null, string encoding = "utf-8", int timeOutSecond = -1)
        {
            string result = string.Empty;
            try
            {
                SetHeader(headers);

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
                {
                    httpContent = new FormUrlEncodedContent(dataDic);
                }

                if (httpContentType == HttpContentType.JSON && dataDic != null)
                {
                    var pamstr = JsonHelper.Serialize(dataDic);
                    httpContent = new StringContent(pamstr, Encoding.GetEncoding(encoding), "application/json");
                }

                if (httpContentType == HttpContentType.QueryString && dataDic != null)
                {
                    StringBuilder sb = new StringBuilder();
                    dataDic.ForEach(x => { if (!string.IsNullOrEmpty(x.Value)) sb.Append(x.Key).Append("=").Append(x.Value).Append("&"); });

                    if (!url.Contains("?"))
                        url += "?";
                    if (url.Contains("&"))
                        url += "&";

                    url = $"{url}{sb.ToString().TrimEnd('&')}";
                }

                CancellationTokenSource cts = new CancellationTokenSource();
                if (timeOutSecond > 0)
                    cts.CancelAfter(timeOutSecond * 1000);

                using (HttpResponseMessage response = await _httpClient.PostAsync(url, httpContent, cts.Token))
                {
                    response.EnsureSuccessStatusCode();
                    result = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetExcetionMessage());
                throw ex;
            }

            return result;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="files"></param>
        /// <param name="headers"></param>
        /// <param name="encoding"></param>
        /// <param name="timeOutSecond"></param>
        /// <returns></returns>
        public static async Task<string> PostFileAsync(string url, List<PostFileDto> files, Dictionary<string, string> headers = null, string encoding = "utf-8", int timeOutSecond = -1)
        {
            string result = string.Empty;
            try
            {
                SetHeader(headers);
                var fromData = new MultipartFormDataContent("----" + Guid.NewGuid().ToString().Replace("-", ""));

                foreach (var f in files)
                {
                    int len = (int)f.Stream.Length;

                    //byte[] bt = new byte[len];
                    byte[] bt = ArrayPool<byte>.Shared.Rent(len);
                    f.Stream.Read(bt, 0, len);

                    var fileContent = new ByteArrayContent(bt);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(f.ContentType);

                    //解决中文乱码
                    var hv = $"form-data; name=\"file\"; filename=\"{f.FileName}\""; 
                    byte[] bdis = Encoding.GetEncoding(encoding).GetBytes(hv);
                    hv = "";
                    foreach (byte b in bdis)
                        hv += (char)b;

                    fileContent.Headers.Add("Content-Disposition", hv);
                    fromData.Add(fileContent, f.FileName, f.FileName);

                    ArrayPool<byte>.Shared.Return(bt, true);
                }
                //var boundary = fromData.Headers.ContentType.Parameters.Single(p => p.Name == "boundary");
                //boundary.Value = boundary.Value.Replace("\"", string.Empty);

                HttpContent httpContent = fromData;

                CancellationTokenSource cts = new CancellationTokenSource();
                if (timeOutSecond > 0)
                    cts.CancelAfter(timeOutSecond * 1000);

                using (HttpResponseMessage response = await _httpClient.PostAsync(url, httpContent, cts.Token))
                {
                    response.EnsureSuccessStatusCode();
                    result = await response.Content.ReadAsStringAsync();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetExcetionMessage());
                throw ex;
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
        /// <returns></returns>
        public static async Task<string> PostAsync(string url, string dataStr, HttpContentType httpContentType = HttpContentType.JSON, Dictionary<string, string> headers = null, string encoding = "utf-8", int timeOutSecond = -1)
        {
            string result = string.Empty;
            try
            {
                SetHeader(headers);

                HttpContent httpContent = null;

                if (dataStr.IsHasValue() && httpContentType != HttpContentType.QueryString)
                {
                    Encoding _encoding = Encoding.GetEncoding(encoding);
                    string mediaType = httpContentType == HttpContentType.JSON ? "application/json" : "application/x-www-form-urlencoded";
                    httpContent = new StringContent(dataStr, _encoding, mediaType);
                }
                if (dataStr.IsHasValue() && httpContentType == HttpContentType.QueryString)
                {
                    if (!url.Contains("?") && !dataStr.StartsWith("?"))
                        url += "?";
                    url = url + dataStr;
                }

                CancellationTokenSource cts = new CancellationTokenSource();
                if (timeOutSecond > 0)
                    cts.CancelAfter(timeOutSecond * 1000);

                using (HttpResponseMessage response = await _httpClient.PostAsync(url, httpContent, cts.Token))
                {
                    response.EnsureSuccessStatusCode();
                    result = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetExcetionMessage());
                throw ex;
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
        /// <returns></returns>
        public static async Task<string> GetAsync(string url, Dictionary<string, string> prams, Dictionary<string, string> headers = null, int timeOutSecond = 5)
        {
            //string result = string.Empty;
            //try
            //{
            //SetHeader(headers);

            StringBuilder sb = new StringBuilder();
            if (prams != null)
            {
                prams.ForEach(x => { if (!string.IsNullOrEmpty(x.Value)) sb.Append(x.Key).Append("=").Append(x.Value).Append("&"); });
                //if (!url.Contains("?"))
                //    url += "?";
                //if (url.Contains("&"))
                //    url += "&";
                //url = $"{url}{sb.ToString().TrimEnd('&')}";
            }

            return await GetAsync(url, sb.ToString(), headers, timeOutSecond);

            //    using (var response = await _httpClient.GetAsync(url))
            //    {
            //        response.EnsureSuccessStatusCode();

            //        result = await response.Content.ReadAsStringAsync();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.GetExcetionMessage());
            //    throw ex;
            //}

            //return result;
        }
        /// <summary>
        /// http get
        /// </summary>
        /// <param name="url"></param>
        /// <param name="pramstr"></param>
        /// <param name="timeOutSecond">default 5 second</param>
        /// <returns></returns>
        public static async Task<string> GetAsync(string url, string pramstr, int timeOutSecond = 5) => await GetAsync(url, pramstr, headers: null, timeOutSecond);
        /// <summary>
        /// http get
        /// </summary>
        /// <param name="url"></param>
        /// <param name="timeOutSecond"></param>
        /// <returns></returns>
        public static async Task<string> GetAsync(string url, int timeOutSecond) => await GetAsync(url, null, timeOutSecond);

        /// <summary>
        /// Get请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="pramstr"></param>
        /// <param name="headers"></param>
        /// <param name="timeOutSecond">default 5 second</param>
        /// <returns></returns>
        public static async Task<string> GetAsync(string url, string pramstr = null, Dictionary<string, string> headers = null, int timeOutSecond = 5)
        {
            string result = string.Empty;
            try
            {
                SetHeader(headers);

                if (pramstr.IsHasValue())
                {
                    if (!url.Contains("?") && !pramstr.Contains("?"))
                        url += "?";
                    if (!url.Contains("?") && pramstr.Contains("?") && (!pramstr.StartsWith("?") || !pramstr.StartsWith("/")))
                        url += "/";
                    if (url.Contains("&") && !pramstr.StartsWith("&"))
                        url += "&";
                    url = $"{url}{pramstr}";
                }

                CancellationTokenSource cts = new CancellationTokenSource();
                if (timeOutSecond > 0)
                {
                    cts.CancelAfter(timeOutSecond * 1000);
                }

                using (var response = await _httpClient.GetAsync(url, cts.Token))
                {
                    response.EnsureSuccessStatusCode();
                    result = await response.Content.ReadAsStringAsync();
                }
            }
            catch (TaskCanceledException ex)
            {
                throw new TimeoutException();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetExcetionMessage());
                throw ex;
            }

            return result;
        }

        public static async Task<HttpResponseDto> SendAsync(string url, HttpMethod httpMethod, HttpContent content)
        {
            HttpResponseDto ret = new HttpResponseDto();
            var request = new HttpRequestMessage(httpMethod, url);
            //request.Content = new FormUrlEncodedContent(kv);
            //_httpClient.DefaultRequestHeaders.ConnectionClose = true;
            //_httpClient.Timeout = TimeSpan.FromSeconds(3);
            request.Content = content;
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var res = await response.Content.ReadAsStringAsync();
                ret.Data = res;
                ret.Success = true;
                return ret;
            }
            ret.Success = false;
            ret.Data = response.StatusCode.ToString();
            return ret;
        }


        static void SetHeader(Dictionary<string, string> headers)
        {
            _httpClient.DefaultRequestHeaders.Clear();

            if (headers == null || headers.Count <= 0)
                return;

            headers.ForEach(x => _httpClient.DefaultRequestHeaders.Add(x.Key, x.Value));
        }

        #region Common Request

        /***
        public T Request<T>(string relyonNo, string issn, long uid, int areaid, string map_id = "", string giftId = "", DateTime? start_time = null, DateTime? end_time = null) where T : new()
        {
            var rey = _commonService.GetTaskRelyon(issn, relyonNo);
            if (rey == null || string.IsNullOrWhiteSpace(rey.Url))
                throw new ErrorException(ErrorCode.ConfigError);

            string parms = rey.Parms;
            string url = rey.Url;
            if (!string.IsNullOrWhiteSpace(parms))
            {
                if (!url.Contains("?") && !parms.Contains("?"))
                    url += "?";

                url += parms;
            }

            url = url.Replace("{zid}", areaid.ToString())
                     .Replace("{iuin}", uid.ToString())
                     .Replace("{map_id}", map_id)
                     .Replace("{start_time}", start_time?.ToString(1))
                     .Replace("{end_time}", end_time?.ToString(1))
                     .Replace("{start_date}", start_time?.ToString(2))
                     .Replace("{end_date}", end_time?.ToString(2))
                     .Replace("{item_type_id}", giftId)
                     .Replace("{action_date}", start_time?.ToString(2));


            string outRet = HttpClientUtil.GetAsync(url).Result;

            JObject obj = JObject.Parse(outRet);
            if (obj == null || obj[rey.CodeField].ToObject<int>() != rey.CodeValue || obj[rey.DataField] == null)
                return default(T);

            return JsonHelper.Deserialize<T>(obj[rey.DataField].ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="relyonNo"></param>
        /// <param name="conditionKey">批量列表接口需要该参数</param>
        /// <param name="issn"></param>
        /// <param name="uid"></param>
        /// <param name="areaid"></param>
        /// <param name="start_time"></param>
        /// <param name="end_time"></param>
        /// <param name="map_id"></param>
        /// <param name="giftId"></param>
        /// <param name="listdata"></param>
        /// <returns></returns>
        public T? RequestApi<T>(string relyonNo, string conditionKey, string issn, long uid, int areaid, DateTime? start_time = null, DateTime? end_time = null, string map_id = "", string giftId = "", string listdata = null) where T : struct
        {
            try
            {
                var rey = _commonService.GetTaskRelyon(issn, relyonNo);
                if (rey == null || string.IsNullOrWhiteSpace(rey.Url) || string.IsNullOrWhiteSpace(rey.Parms))
                    throw new ErrorException(ErrorCode.ConfigError, "rey 配置错误");

                if (rey.IsBatch && conditionKey.IsNullOrEmpty())
                    throw new ErrorException(ErrorCode.ConfigError, "conditionKey 未配置");

                if (!start_time.HasValue || !end_time.HasValue)
                {
                    var a = _activeService.GetActiveMaster(issn);
                    if (!start_time.HasValue) start_time = a.BeginTime;
                    if (!end_time.HasValue) end_time = a.EndTime;
                }
                string parms = rey.Parms;
                string url = rey.Url;
                if (!string.IsNullOrWhiteSpace(parms))
                {
                    if (!url.Contains("?") && !parms.Contains("?"))
                        url += "?";

                    url += parms;
                }

                url = url.Replace("{zid}", areaid.ToString())
                         .Replace("{iuin}", uid.ToString())
                         .Replace("{map_id}", map_id)
                         .Replace("{start_time}", start_time?.ToString(1))
                         .Replace("{end_time}", end_time?.ToString(1))
                         .Replace("{start_date}", start_time?.ToString(2))
                         .Replace("{end_date}", end_time?.ToString(2))
                         .Replace("{item_type_id}", giftId)
                         .Replace("{action_date}", start_time?.ToString(2))
                         .Replace("{list_data}", listdata);


                string outRet = HttpClientUtil.GetAsync(url).Result;
                JObject obj = JObject.Parse(outRet);
                if (obj == null || obj[rey.CodeField].ToObject<int>() != rey.CodeValue)
                    return null;

                string[] fileds = rey.DataField.Split(":");
                string tmpVal = outRet;
                JToken jobj = obj;
                for (int i = 0; i < fileds.Length; i++)
                {
                    if (jobj.Type == JTokenType.Object)
                        jobj = jobj[fileds[i]];
                    else if (jobj.Type == JTokenType.Array)
                    {
                        jobj = jobj.FirstOrDefault();
                        jobj = jobj[fileds[i]];
                    }
                }
                if (jobj == null) return null;

                if (rey.IsBatch)
                {
                    if (jobj.Type == JTokenType.Array)
                    {
                        foreach (JToken o in jobj)
                        {
                            if (o["task_type"].ToString() == conditionKey)
                            {
                                return ConvertHelper.ConvertTo<T>(o["task_status"].ToString());
                            }
                        }
                        return default(T);
                    }
                }
                tmpVal = jobj.ToString();
                return ConvertHelper.ConvertTo<T>(tmpVal);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{relyonNo}:{ex.Message}");
                return default(T);
            }
        }
        **/

        #endregion

        /// <summary>
        /// HttpContentType
        /// </summary>
        //public enum HttpContentType
        //{

        //    QueryString = 0,

        //    JSON = 1,

        //    FormData = 2
        //}
    }
    /// <summary>
    /// 上传文件
    /// </summary>
    public class PostFileDto
    {
        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// image/jpeg
        /// if (!new FileExtensionContentTypeProvider().Mappings.TryGetValue(Path.GetExtension(f.FileName), out var contenttype)) throw new Exception("文件格式不存在");
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// 文件流
        /// </summary>
        public Stream Stream { get; set; }
    }

    public class HttpResponseDto
    {
        public bool Success { get; set; }
        public string Data { get; set; }
        public string StatusCode { get; set; }
    }

    /***
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
    **/
}
