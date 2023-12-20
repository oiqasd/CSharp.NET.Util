// ****************************************************
// * 创建日期：
// * 创建人：y
// * 备注：
// ****************************************************

using System;
using System.Runtime.Serialization;

namespace CSharp.Net.Util
{
    public class AppException : Exception
    {
        /// <summary>
        /// 错误码
        /// </summary>
        public object ErrorCode { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public object ErrorMessage { get; set; }

        /// <summary>
        /// 状态码
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// 额外数据
        /// </summary>
        public new object Data { get; set; }

        public AppException() { }

        public AppException(object code, string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = code;
            ErrorMessage = message;
        }
        public AppException(string message) : base(message)
        {
            ErrorMessage = message;
        }

        public AppException(string message, Exception innerException) : base(message, innerException)
        {
            ErrorMessage = message;
        }

        protected AppException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
