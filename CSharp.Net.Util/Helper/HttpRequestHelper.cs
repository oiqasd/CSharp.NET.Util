using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    /// <summary>
    /// Http请求帮助类,
    /// 已废弃,请使用HttpClientUtil
    /// </summary>
    [Obsolete("已弃用2021/8/15，推荐使用HttpClientUtil代替")]
    public class HttpRequestHelper
    {
        /// <summary>
        /// 发生post请求
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="postDataStr"></param>
        /// <returns></returns>
        public static async Task<string> DoHttpPostAsync(string Url, string postDataStr, HttpContentType contentType = HttpContentType.JSON, int timeOut = 300000)
        {
            HttpResquestEntity requestEntity = new HttpResquestEntity()
            {
                Url = Url,
                SendData = postDataStr,
                Timeout = timeOut,
                ContentType = contentType
            };

            HttpWebRequest request = CreateRequest(requestEntity);
            //using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (var response = await request.GetResponseAsync())
            {
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                return retString;
            }
        }

        /// <summary>
        /// 只发送不接收
        /// </summary>
        /// <param name="resquestEntity"></param>
        public static void SendOnly(HttpResquestEntity resquestEntity)
        {
            HttpWebRequest request = CreateRequest(resquestEntity);
            try
            {
                request.GetResponse().Close();//销毁关闭响应
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("操作超时"))
                    throw ex;
            }
        }

        /// <summary>
        /// 只是获取响应码
        /// </summary>
        /// <param name="resquestEntity"></param>
        /// <returns></returns>
        public static HttpStatusCode SendStatus(HttpResquestEntity resquestEntity)
        {
            HttpWebRequest request = CreateRequest(resquestEntity);

            try
            {
                using (HttpWebResponse wr = (HttpWebResponse)request.GetResponse())
                {
                    //返回状态
                    return wr.StatusCode;
                }
            }
            catch (WebException ex)
            {
                return GetHttpStatusCode(ex);
            }
        }

        /// <summary>
        /// 获取响应信息
        /// </summary>
        /// <param name="resquestEntity"></param>
        /// <returns></returns>
        public static HttpResponseEntity SendValue(HttpResquestEntity resquestEntity)
        {
            HttpResponseEntity responseEntity = new HttpResponseEntity();
            string responseStr = "";
            HttpStatusCode httpStatusCode;
            HttpWebRequest request = CreateRequest(resquestEntity);

            try
            {
                using (HttpWebResponse wr = (HttpWebResponse)request.GetResponse())
                {
                    httpStatusCode = wr.StatusCode;
                    Stream dataStream = wr.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream, resquestEntity.URLEncoding);
                    responseStr = reader.ReadToEnd();
                    reader.Close();
                    dataStream.Close();

                }
            }
            catch (WebException ex)
            {
                httpStatusCode = GetHttpStatusCode(ex);
                responseStr += "";
                responseEntity.ErrorMsg = ex.Message;
            }
            catch (Exception e)
            {
                httpStatusCode = HttpStatusCode.BadRequest;
                responseStr += "";
                responseEntity.ErrorMsg = e.Message;
            }
            responseEntity.ResponseContext = responseStr;
            responseEntity.ResponseHttpStatusCode = httpStatusCode;
            return responseEntity;
        }

        /// <summary>
        /// 发生get请求
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="postDataStr"></param>
        /// <returns></returns>
        public static string HttpGet(string Url, string postDataStr)
        {
            HttpResquestEntity requestEntity = new HttpResquestEntity()
            {
                Url = Url,
                SendData = postDataStr,
                MethodType = HttpMethodType.GET
            };

            HttpWebRequest request = CreateRequest(requestEntity);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }
        /// <summary>
        /// 创建请求对象
        /// </summary>
        /// <param name="resquestEntity"></param>
        /// <returns></returns>
        private static HttpWebRequest CreateRequest(HttpResquestEntity resquestEntity)
        {

            if (resquestEntity.Url.ToLower().StartsWith("https"))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);//验证服务器证书回调自动验证
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            }
            HttpWebRequest request;

            if (resquestEntity.MethodType == HttpMethodType.GET)
            {
                string getUrl = resquestEntity.Url + "?" + resquestEntity.SendData;
                request = (HttpWebRequest)HttpWebRequest.Create(getUrl);
                request.Method = "GET";
                request.ContentType = "text/html;charset=UTF-8";
                request.Timeout = resquestEntity.Timeout;
            }
            else
            {
                request = (HttpWebRequest)HttpWebRequest.Create(resquestEntity.Url);

                if (!string.IsNullOrEmpty(resquestEntity.HeaderKey))
                {
                    request.Headers.Add(resquestEntity.HeaderKey, resquestEntity.HeaderValue);
                    request.Accept = "application/json";
                }

                request.ContentType = GetContentTypeName(resquestEntity.ContentType) + "charset=" + resquestEntity.URLEncoding.HeaderName;
                request.Method = "POST";// EnumHelper.GetName<string>(resquestEntity.MethodType);

                byte[] bData = (resquestEntity.URLEncoding.GetBytes(resquestEntity.SendData));
                request.ContentLength = bData.Length;
                request.Timeout = resquestEntity.Timeout;
                Stream writeStream = request.GetRequestStream();
                writeStream.Write(bData, 0, bData.Length);
                writeStream.Close();
            }
            return request;
        }

        public static HttpStatusCode GetHttpStatusCode(WebException ex)
        {
            try
            {
                //返回错误状态
                if (ex.Response != null)
                {
                    return ((HttpWebResponse)ex.Response).StatusCode;// Status.ToString();                   
                }
                else
                {
                    throw ex;
                }
            }
            catch (Exception ex1)
            {
                throw ex1;
            }
        }

        public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {   // 总是接受  
            return true;
        }

        /// <summary>
        /// 读取application/json格式传递过来的字符串
        /// </summary>
        /// <param name="Context"></param>
        /// <returns></returns>
        //public static String GetJson(HttpRequest request, Encoding encoding = null)
        //{
        //    if (encoding == null)
        //        encoding = Encoding.Default;
        //    string postContent = "";
        //    if (request != null)
        //    {
        //        Stream postData = request.Body;
        //        using (StreamReader sRead = new StreamReader(postData, encoding))
        //        {
        //            postContent = sRead.ReadToEnd();
        //            sRead.Close();
        //        }
        //    }
        //    return HttpUtility.HtmlDecode(postContent);

        //}

        public static string GetContentTypeName(HttpContentType contentType)
        {
            switch (contentType)
            {
                case HttpContentType.JSON:
                    return "application/json;";
                case HttpContentType.QueryString:
                case HttpContentType.x_www_form_urlEncoded:
                    return "application/x-www-form-urlencoded;";
            }
            return "application/x-www-form-urlencoded;";
        }

    }

    /// <summary>
    /// 请求内容类型
    /// </summary>
    public enum HttpContentType
    {

        QueryString = 0,// "application/x-www-form-urlencoded;",

        JSON = 1,//"application/json;"

        /// <summary>
        /// multipart/form-data:既可上传文件等二进制数据，也可以上传表单键值对
        /// x-www-form-urlencoded:只能上传键值对，且键值对都是间隔分开的(name=zhang&age = 23)
        /// </summary>
        FormData = 2,
        /// <summary>
        /// x-www-form-urlencoded
        /// </summary>
        x_www_form_urlEncoded = 3 ,
    }

    /// <summary>
    /// 获取请求数据方式
    /// </summary>
    public enum HttpMethodType
    {
        /// <summary>
        /// Post方式
        /// </summary>
        POST = 0,//"POST",

        /// <summary>
        /// Get方式
        /// </summary>
        GET = 1//"GET"
    }

    /// <summary>
    /// Http请求实体
    /// </summary>
    public class HttpResquestEntity
    {
        public HttpResquestEntity()
        {
            MethodType = HttpMethodType.POST;
            URLEncoding = Encoding.UTF8;
            Timeout = 30000;
            ContentType = HttpContentType.JSON;
        }

        public string Url { get; set; }

        public string SendData { get; set; }

        public HttpMethodType MethodType { get; set; }

        public Encoding URLEncoding { get; set; }

        public int Timeout { get; set; }

        public string HeaderKey { get; set; }

        public string HeaderValue { get; set; }

        public HttpContentType ContentType { get; set; }

    }

    /// <summary>
    /// Http响应内容
    /// </summary>
    public class HttpResponseEntity
    {
        public HttpStatusCode ResponseHttpStatusCode { get; set; }

        public string ResponseContext { get; set; }

        /// <summary>
        /// 响应的异常信息
        /// </summary>
        public string ErrorMsg { get; set; }
    }
}