using CSharp.Net.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CSharp.Net.Mvc.Filters
{
    internal class ExceptionHandle : IAsyncExceptionFilter
    {
        private ILogger _logger;
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
                requestInput = await request.ReadBodyAsync();
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

                var strLog = $"[IP:{context.HttpContext.GetRemoteIP()}]," +
                             $"[接口:{method_name}]," +
                             $"[参数:{requestInput}]," +
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
    }
}
