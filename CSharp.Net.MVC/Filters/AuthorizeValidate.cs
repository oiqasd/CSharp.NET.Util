using CSharp.Net.Mvc;
using CSharp.Net.Util;
using CSharp.Net.Util.Validate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CSharp.Net.Mvc.Filters
{
    public class AuthorizeValidate : ActionFilterAttribute
    {
        private ILogger _logger;
        private IGeneral _general;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IConfiguration _configuration;
        private IUserApplication _userApplication;
        private ICache _cache;

        private bool _isNeedLog;
        private static readonly string key = "enterTime";

        ValueStopwatch stopwatch;

        public AuthorizeValidate(ILogger<AuthorizeValidate> logger, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IGeneral general, IRedisCache cache, IUserApplication userApplication)
        {
            _logger = logger;
            _general = general;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _isNeedLog = Convert.ToBoolean(_configuration["Logger:IsOpening"]);
            _userApplication = userApplication;
            _cache = cache;
        }
        /***
        // 同步和异步不能同时重写
        /// <summary>
        /// 和OnResultExecuting、OnResultExecuted不能同时重写
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            await base.OnResultExecutionAsync(context, next);//没有其它意义 基类判断了空参

            //异步读取body  替代设置AllowSynchronousIO=true
            //重置读取
            context.HttpContext.Request.EnableBuffering();

            StreamReader stream = new StreamReader(context.HttpContext.Request.Body);
            string body = await stream.ReadToEndAsync();
            //重置Position
            context.HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);

            await next();
        }
        /// <summary>
        /// OnActionExecutionAsync和OnActionExecuting、OnActionExecuted不能同时重写
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await base.OnActionExecutionAsync(context, next);//没有其它意义 基类判断了空参
            await context.HttpContext.Response.WriteAsync("OnActionExecutionAsync");
            await next();
        }
        **/

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //设置AllowSynchronousIO
            //var syncIOFeature = context.HttpContext.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpBodyControlFeature>();
            //if (syncIOFeature != null)
            //{
            //    syncIOFeature.AllowSynchronousIO = true;
            //}
            stopwatch = ValueStopwatch.StartNew();

            //记录进入请求的时间
            context.HttpContext.Request.Headers[key] = DateTime.Now.ToString();

            object parmObj = null;
            PropertyInfo parmPro = null;

            // 去除不用判断的类型
            //string[] existsActions = new string[] { "AllowFile" };
            string[] exceptActions = new string[] { nameof(AllowAnonymousAttribute) };
            {
                //var mi = context.ActionDescriptor.GetType().GetProperty("MethodInfo");
                //var isActionAllow = ((MethodInfo)mi.GetValue(context.ActionDescriptor)).GetCustomAttributes<AllowAnonymousAttribute>().Any();

                //var isControllerAllow = context.Controller.GetType().GetCustomAttributes<AllowAnonymousAttribute>().Any();
                //return isActionAllow || isControllerAllow;
            }

            // Action上用到的标签
            //var attrList = ((Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)context.ActionDescriptor).MethodInfo.GetCustomAttributes().Select(s => s.ToString().Substring(s.ToString().LastIndexOf('.') + 1).Replace("Attribute", "")).ToArray();
            var attrList = context.ActionDescriptor.EndpointMetadata.Select(e => e.GetType().Name).ToList();

            //ICurrentUser tm = _general.GetCurrentUser();
            //if (!attrList.Contains(nameof(AllowAnonymousAttribute)))
            //{
            //    if (tm == null || tm.Uid <= 0)
            //    {
            //        throw new AppException(ReturnCode.UnAuthorized, ReturnCode.UnAuthorized.GetDescription());
            //    }

            //    var user = _userApplication.GetUser(tm.Uid);
            //    if (user.IsLock)
            //    {
            //        throw new AppException(ReturnCode.UserAccountLocked, ReturnCode.UserAccountLocked.GetDescription());
            //        //var result = new Response<string>(ErrorCode.User_Account_Locked);
            //        //context.Result = new JsonResult(result);
            //        //return;
            //    }
            //}

            // 去除类型
            if (attrList.Intersect(exceptActions).Count() > 0)
                return;

            // 验证参数
            if (context.ActionArguments.Count > 0)
            {
                parmObj = context.ActionArguments.FirstOrDefault().Value;
                // 传入参数不对
                parmPro = parmObj.GetType().GetProperties().FirstOrDefault(q => q.Name == "Parameter");
                if (parmPro == null)
                {
                    throw new AppException(ReturnCode.PamramerError, ReturnCode.PamramerError.GetDescription());
                }

                var parameter = parmPro.GetValue(parmObj);
                if (parameter == null)
                {
                    var type = parmPro.PropertyType;
                    if (type.GetConstructor(Type.EmptyTypes) != null)
                    {
                        //实例化请求对象
                        parmPro.SetValue(parmObj, Activator.CreateInstance(type));
                    }
                }
            }


            // 验证DTO参数模型
            if (attrList.Contains(nameof(ValidateParameterAttribute)))
            {
                var parameter = parmPro.GetValue(parmObj);
                var result = Validate.ValidataData(parameter);
                if (!result.Flag)
                {
                    throw new AppException(ReturnCode.PamramerError, result.ErrorMessage);
                }
            }

            //防并发提交
            if (context.HttpContext.Request.Method.ToLower() == "post" && parmObj != null)
            {
                string ckey = $"{tm?.Uid}:{context.HttpContext.Request.Path}:{JsonHelper.Serialize(parmPro.GetValue(parmObj))}";
                if (_cache.KeyExists(ckey))
                {
                    throw new AppException(ReturnCode.RepeatCommit, ReturnCode.RepeatCommit.GetDescription());
                }
                _cache.StringSet(ckey, "", 1, false);
            }


        }

        /// <summary>
        /// OnActionExecuted
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            // 统一处理输出对象数据
            if (((Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)context.ActionDescriptor).MethodInfo.GetCustomAttributes().FirstOrDefault(q => q.ToString().Contains("AllowFile")) != null)
            {
                return;
            }

            StringValues beginTime;

            HttpRequest request = context.HttpContext.Request;

            // 写日志
            if (this._isNeedLog)
            {
                /*** 记录输入参数
                string requestInput = null;
                //获取request.Body内容
                if (request.Method.ToLower().Equals("post"))
                {
                    //request.EnableRewind();//.net core 2.2
                    request.EnableBuffering();//.net 3.0
                    request.Body.Position = 0;
                    var streamReader = new StreamReader(request.Body);
                    requestInput = streamReader.ReadToEnd();
                }
                else if (request.Method.ToLower().Equals("get"))
                {
                    requestInput = request.QueryString.Value;
                }
                **/

                if (context.HttpContext.Request.Headers.TryGetValue(key, out beginTime))
                {
                    DateTime endTime = DateTime.Now;
                    DateTime startTime = DateTime.Parse(beginTime.ToString());

                    var client_ip = _httpContextAccessor?.HttpContext.Connection.RemoteIpAddress?.ToString();
                    var method_name = context.HttpContext.Request.Path;
                    // var duration = (float)(endTime - startTime).TotalMilliseconds;
                    var duration = stopwatch.GetElapsedTime().TotalMilliseconds;
                    var flag = "正常";
                    if (duration >= 2000)
                    {
                        flag = "*****接口缓慢*****";
                    }

                    var strLog = $"[IP:{client_ip}],[接口:{method_name}],[响应时间:{duration},{flag}]";
                    Task.Run(() =>
                    {
                        _logger.LogInformation(strLog);
                    });
                }
            }

            if (context.Exception != null)
            {
                return;
            }

            // Get成功请求不需要Message
            var objectResult = context.Result as ObjectResult;
            if (objectResult.StatusCode != null)
            {
                Response<string> responseObj = new Response<string>(ReturnCode.PamramerError);
                context.Result = new JsonResult(responseObj);
                return;
            }

            if (objectResult.Value == null)
            {
                try
                {
                    var resultType = objectResult.DeclaredType;
                    if (ReflectUtil.HasDefaultConstructor(resultType))
                    {
                        var instance = Activator.CreateInstance(resultType);
                        objectResult.Value = instance;
                    }
                    var d = ReflectUtil.GetConstructorInfo(resultType);
                    var r = resultType.GetConstructors().OrderBy(x => x.GetParameters().Length - x.GetParameters().Count(p => p.IsOptional)).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }


            var retType = objectResult.Value.GetType();//Response

            var codeProperty = objectResult.Value.GetType().GetProperties().FirstOrDefault(q => q.Name == "Code");
            if (codeProperty == null)
            {
                //不是Response返回的这里是空
                return;
            }

            var messageProperty = objectResult.Value.GetType().GetProperties().FirstOrDefault(q => q.Name == "Message");
            if (codeProperty.GetValue(objectResult.Value)?.ToString() == "100" && context.HttpContext.Request.Method == "GET")
            {
                messageProperty.SetValue(objectResult.Value, "");
            }

            // 处理结果为Null转成默认值
            var dataProperty = objectResult.Value.GetType().GetProperties().FirstOrDefault(q => q.Name == "Data");
            if (dataProperty != null && dataProperty.GetValue(objectResult.Value) == null)
            {
                var type = dataProperty.PropertyType;
                switch (type.Name)
                {
                    case "String":
                        dataProperty.SetValue(objectResult.Value, "");
                        break;
                    case "IEnumerable`1":
                        Type enumerableType = typeof(List<>);
                        enumerableType = enumerableType.MakeGenericType(type.GenericTypeArguments);
                        var o = Activator.CreateInstance(enumerableType);
                        dataProperty.SetValue(objectResult.Value, o);
                        break;
                    default:
                        var instance = Activator.CreateInstance(type);
                        dataProperty.SetValue(objectResult.Value, instance);
                        break;
                }
            }
        }


        public override void OnResultExecuting(ResultExecutingContext context)
        {
            // 根据实际需求进行具体实现

            // 数据脱敏

            // 统一处理输出对象数据      
        }

    }
}

