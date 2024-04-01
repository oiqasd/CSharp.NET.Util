#if NET6_0_OR_GREATER
using CSharp.Net.Util.AException;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    public class ExceptionOps
    {
        /// <summary>
        /// 方法错误异常特性
        /// </summary>
        private static readonly ConcurrentDictionary<MethodBase, MethodAException> ErrorMethods;
        /// <summary>
        /// 错误代码类型
        /// </summary>
        private static readonly IEnumerable<Type> ErrorCodeTypes;
        /// <summary>
        /// 错误消息字典
        /// </summary>
        private static readonly ConcurrentDictionary<string, string> ErrorCodeMessages;

        /// <summary>
        /// 构造函数
        /// </summary>
        static ExceptionOps()
        {
            ErrorMethods = new ConcurrentDictionary<MethodBase, MethodAException>();
          
            ErrorCodeTypes = GetErrorCodeTypes();
            ErrorCodeMessages = GetErrorCodeMessages();
        }

        #region


        /// <summary>
        /// 抛出业务异常信息
        /// </summary>
        /// <param name="errorMessage">异常消息</param>
        /// <param name="args">String.Format 参数</param>
        /// <returns>异常实例</returns>
        public static AppException Throw(string errorMessage, params object[] args)
        {
            var exception = Out(errorMessage, typeof(ValidationException), args).ErrorCode(400);
            throw exception;
        }

        /// <summary>
        /// 抛出业务异常信息
        /// </summary>
        /// <param name="errorCode">错误码</param>
        /// <param name="args">String.Format 参数</param>
        /// <returns>异常实例</returns>
        public static AppException Throw(object errorCode, params object[] args)
        {
            var exception = Out(errorCode, typeof(ValidationException), args).ErrorCode(400);

            throw exception;
        }

        /// <summary>
        /// 抛出字符串异常
        /// </summary>
        /// <typeparam name="TException">具体异常类型</typeparam>
        /// <param name="errorMessage">异常消息</param>
        /// <param name="args">String.Format 参数</param>
        /// <returns>异常实例</returns>
        public static AppException Out<TException>(string errorMessage, params object[] args)
            where TException : class
        {
            return Out(errorMessage, typeof(TException), args);
        }


        /// <summary>
        /// 抛出错误码异常
        /// </summary>
        /// <param name="errorCode">错误码</param>
        /// <param name="exceptionType">具体异常类型</param>
        /// <param name="args">String.Format 参数</param>
        /// <returns>异常实例</returns>
        public static AppException Out(object errorCode, Type exceptionType, params object[] args)
        {
            var (ErrorCode, Message) = GetErrorCodeMessage(errorCode, args);
            return new AppException(errorCode, Message,
                Activator.CreateInstance(exceptionType, new object[] { Message }) as Exception)
            { ErrorCode = ErrorCode };
        }

        #endregion

        /// <summary>
        /// 获取错误码消息
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static (object ErrorCode, string Message) GetErrorCodeMessage(object errorCode, params object[] args)
        {
            errorCode = HandleEnumErrorCode(errorCode);

            // 获取出错的方法
            var methodAException = GetEndPointExceptionMethod();

            // 获取当前状态码匹配异常特性
            var ifExceptionAttribute = methodAException.AExceptionAttributes.FirstOrDefault(u => u.ErrorCode != null && HandleEnumErrorCode(u.ErrorCode).ToString().Equals(errorCode.ToString()));

            // 获取错误码消息
            var errorCodeMessage = ifExceptionAttribute == null || string.IsNullOrWhiteSpace(ifExceptionAttribute.ErrorMessage)
                ? (ErrorCodeMessages.GetValueOrDefault(errorCode.ToString()))
                : ifExceptionAttribute.ErrorMessage;

            // 如果所有错误码都获取不到，则找全局 [AException] 错误
            if (string.IsNullOrWhiteSpace(errorCodeMessage))
            {
                errorCodeMessage = methodAException.AExceptionAttributes.FirstOrDefault(u => u.ErrorCode == null && !string.IsNullOrWhiteSpace(u.ErrorMessage))?.ErrorMessage;
            }

            // 字符串格式化
            return (errorCode, StringHelper.Render(errorCodeMessage));
        }

        /// <summary>
        /// 处理枚举类型错误码
        /// </summary>
        /// <param name="errorCode">错误码</param>
        /// <returns></returns>
        private static object HandleEnumErrorCode(object errorCode)
        {
            // 获取类型
            var errorType = errorCode.GetType();

            // 判断是否是内置枚举类型，如果是解析特性
            if (ErrorCodeTypes != null && ErrorCodeTypes.Any(u => u == errorType))
            {
                var fieldinfo = errorType.GetField(Enum.GetName(errorType, errorCode));
                if (fieldinfo.IsDefined(typeof(ErrorCodeItemMetadataAttribute), true))
                {
                    errorCode = GetErrorCodeItemInformation(fieldinfo).Key;
                }
            }
            return errorCode;
        }

        /// <summary>
        /// 获取错误代码信息
        /// </summary>
        /// <param name="fieldInfo">字段对象</param>
        /// <returns>(object key, object value)</returns>
        private static (object Key, string Value) GetErrorCodeItemInformation(FieldInfo fieldInfo)
        {
            var errorCodeItemMetadata = fieldInfo.GetCustomAttribute<ErrorCodeItemMetadataAttribute>();
            return (errorCodeItemMetadata.ErrorCode ?? fieldInfo.Name, errorCodeItemMetadata.ErrorMessage.Format(errorCodeItemMetadata.Args));
        }

        /// <summary>
        /// 获取堆栈中顶部抛异常方法
        /// </summary>
        /// <returns></returns>
        private static MethodAException GetEndPointExceptionMethod(Type type = null)
        {
            // 获取调用堆栈信息
            var stackTrace = EnhancedStackTrace.Current();

            // 获取出错的堆栈信息，在 web 请求中获取控制器或动态API的方法，除外获取第一个出错的方法
            var stackFrame = stackTrace.FirstOrDefault(u => type == null || type.GetType().IsAssignableFrom(u.MethodInfo.DeclaringType))
                ?? stackTrace.FirstOrDefault(u => u.GetMethod().DeclaringType.Namespace != typeof(ExceptionOps).Namespace);

            // 获取出错的方法
            var errorMethod = stackFrame.MethodInfo.MethodBase;

            // 判断是否已经缓存过该方法，避免重复解析
            var isCached = ErrorMethods.TryGetValue(errorMethod, out var methodException);
            if (isCached) return methodException;

            // 获取堆栈中所有的 [AException] 特性
            var aExceptionAttributes = stackTrace
                .Where(u => u.MethodInfo.MethodBase != null && u.MethodInfo.MethodBase.IsDefined(typeof(AExceptionAttribute), true))
                .SelectMany(u => u.MethodInfo.MethodBase.GetCustomAttributes<AExceptionAttribute>(true));

            // 组装方法异常对象
            methodException = new MethodAException
            {
                ErrorMethod = errorMethod,
                AExceptionAttributes = aExceptionAttributes
            };

            // 存入缓存
            ErrorMethods.TryAdd(errorMethod, methodException);

            return methodException;
        }

        /// <summary>
        /// 获取错误代码类型
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Type> GetErrorCodeTypes()
        {
            // 查找所有公开的枚举贴有 [ErrorCodeType] 特性的类型
             
            return new List<Type>();
        }


        /// <summary>
        /// 获取所有错误消息
        /// </summary>
        /// <returns></returns>
        private static ConcurrentDictionary<string, string> GetErrorCodeMessages()
        {
            var defaultErrorCodeMessages = new ConcurrentDictionary<string, string>();

            // 查找所有 [ErrorCodeType] 类型中的 [ErrorCodeMetadata] 元数据定义
            var errorCodeMessages = ErrorCodeTypes.SelectMany(u => u.GetFields()
                                    .Where(u => u.IsDefined(typeof(ErrorCodeItemMetadataAttribute))))
                .Select(u => GetErrorCodeItemInformation(u))
               .ToDictionary(u => u.Key.ToString(), u => u.Value);

            defaultErrorCodeMessages.AddOrUpdate(errorCodeMessages);

            return defaultErrorCodeMessages;
        }
    }
}
#endif