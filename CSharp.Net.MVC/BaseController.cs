using CSharp.Net.Mvc.Filters;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CSharp.Net.Mvc
{
    //[Authorize]
    [Area("api")]
    [Route("[area]/[controller]/[action]")]
    [ApiController]
    [EnableCors("AllowCorsAny")]
    [IngorePrivacy]
    [TypeFilter(typeof(ApiExceptionAttribute))]
    [ApiExplorerSettings(GroupName = nameof(ApiVersionInfo.Default))]
    public class BaseController : ControllerBase
    {
        public ILogger Logger { get; set; }

        /// <summary>
        /// 成功
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        protected Response<T> Success<T>(T data)
        {
            return new Response<T>(data);
        }
        /// <summary>
        /// 成功
        /// </summary>
        /// <returns></returns>
        protected Response Success()
        {
            return new Response(ReturnCode.OK);
        }

        /// <summary>
        /// 失败处理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        protected Response<T> Fail<T>(ReturnCode code, string message = null)
        {
            return new Response<T>(code, message ?? code.GetDescription());
        }

        protected Response Fail(ReturnCode code, string message = null)
        {
            return new Response(code, message);
        }
    }
}
