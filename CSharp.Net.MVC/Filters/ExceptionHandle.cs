using CSharp.Net.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSharp.Net.Mvc.Filters
{
    internal class ExceptionHandle : IAsyncExceptionFilter
    {
        private ILogger _logger;
        // 敏感字段脱敏正则：Password / OldPwd / NewPwd / Token / Sign / Mobile / Phone / Email / AccessKey
        private static readonly Regex _sensitiveFieldRegex = new Regex(
            "\"(password|oldpwd|newpwd|token|sign|accesskey|accessid|mobilephone|phone|email)\"\\s*:\\s*\"[^\"]*\"",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        //private readonly IHttpContextAccessor _httpContextAccessor;
        //private IConfiguration _configuration;
        //private IGeneral _general;

        public ExceptionHandle(ILogger<ExceptionHandle> logger)
        {
            _logger = logger;
            //_general = general;
        }

        public async Task OnExceptionAsync(ExceptionContext context)
        {
            if (context.ExceptionHandled) return;

            var response = context.HttpContext.Response;
            var exception = context.Exception;
            //response.ContentType = "application/json;charset=utf-8";
            //response.ContentType = "text/plain;charset=utf-8";
            //response.ContentType = context.HttpContext.Request.Headers["Accept"];

            HttpRequest request = context.HttpContext.Request;
            string requestInput = null;

            //获取request.Body内容
            if (request.Method == HttpMethods.Post)
                requestInput = await ReadBodyAsync(request);
            else if (request.Method == HttpMethods.Get)
                requestInput = request.QueryString.Value;

            // 写入异常日志
            if (App.Configuration["Logger:IsOpening"].ToBoolean())
            {
                // 获取控制器信息
                //var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

                // 判断是否是验证异常
                //var isValidationException = context.Exception is ErrorException errorException && errorException.ErrorCode != ErrorCode.OK;

                var method_name = context.Exception.TargetSite.Name;

                var sanitizedInput = SanitizeAndTruncate(requestInput, 4096);

                var strLog = $"[IP:{context.HttpContext.GetRemoteIP()}]," +
                             $"[接口:{method_name}]," +
                             $"[参数:{sanitizedInput}]," +
                             $"[错误信息:{context.Exception.Message}]";

                Task.Run(() => _logger.LogError(exception, strLog));
            }

            string errPosition = $"Api：{context.HttpContext.Request.Path}," +
                $"异常位置：{context.Exception.TargetSite.DeclaringType}," +
                $"异常方法：{context.Exception.TargetSite.Name}," +
                $"异常信息：{exception.GetBaseException().Message}," +
                $"异常详细：{exception.GetBaseException().StackTrace}";
            Response<string> result = new Response<string>(ReturnCode.SystemError, errPosition);
            string strJson = JsonHelper.Serialize(result);
            context.ExceptionHandled = true;
            // 记录异常信息日志
            response.ContentType = "text/plain;charset=utf-8";
            await response.WriteAsync(strJson).ConfigureAwait(false);
        }


        /// <summary>
        /// 脱敏并截断请求体字符串，避免敏感字段（密码/Token/手机号等）写入日志。
        /// </summary>
        private static string SanitizeAndTruncate(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var sanitized = _sensitiveFieldRegex.Replace(input, m =>
            {
                var fieldName = m.Groups[1].Value;
                return $"\"{fieldName}\":\"***\"";
            });
            if (sanitized.Length > maxLength)
            {
                sanitized = sanitized.Substring(0, maxLength) + "...(truncated)";
            }
            return sanitized;
        }

        /// <summary>
        /// 读取请求体内容并将流位置复原，供异常日志记录使用。
        /// </summary>
        private static async Task<string> ReadBodyAsync(HttpRequest request)
        {
            if (request.Body == null)
            {
                return null;
            }

            request.EnableBuffering();
            if (request.Body.CanSeek)
            {
                request.Body.Position = 0;
            }

            using var reader = new StreamReader(request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();

            if (request.Body.CanSeek)
            {
                request.Body.Position = 0;
            }

            return body;
        }

        /// <summary>
        /// 获取异常文件行号
        /// </summary>
        /// <param name="exception"></param>
        internal static string ExceptionProfiler(Exception exception)
        {
            string msg = string.Empty;
            if (exception == null) return msg;

            // 获取异常堆栈
            var stackTrace = new StackTrace(exception, true);
            if (stackTrace.FrameCount == 0) return msg;
            var traceFrame = stackTrace.GetFrame(0);

            // 获取出错的文件名
            var exceptionFileName = traceFrame.GetFileName();

            // 获取出错的行号
            var exceptionFileLineNumber = traceFrame.GetFileLineNumber();

            // 打印错误文件名和行号
            if (!string.IsNullOrWhiteSpace(exceptionFileName) && exceptionFileLineNumber > 0)
            {
                msg = $"file:{exceptionFileName},line:{exceptionFileLineNumber},";
            }
            msg += exception.ToString();
            return msg;
        }
    }
}
