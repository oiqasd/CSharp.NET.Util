using System;

namespace CSharp.Net.Util
{
    [ AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class AExceptionAttribute:Attribute
    {

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public AExceptionAttribute()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="errorCode">错误编码</param>
        /// <param name="args">格式化参数</param>
        public AExceptionAttribute(object errorCode, params object[] args)
        {
            ErrorCode = errorCode;
            Args = args;
        }

        /// <summary>
        /// 捕获特定异常类型异常（用于全局异常捕获）
        /// </summary>
        /// <param name="exceptionType"></param>
        public AExceptionAttribute(Type exceptionType)
        {
            ExceptionType = exceptionType;
        }

        /// <summary>
        /// 错误编码
        /// </summary>
        public object ErrorCode { get; set; }

        /// <summary>
        /// 异常类型
        /// </summary>
        public Type ExceptionType { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 格式化参数
        /// </summary>
        public object[] Args { get; set; }
    }
}
