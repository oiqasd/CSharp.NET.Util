// ****************************************************
// * 创建日期：
// * 创建人：y
// * 备注：
// ****************************************************

using System;
using System.Runtime.Serialization;

namespace CSharp.Net.Util
{
    public class BaseException : Exception
    {
        public BaseException() { }

        public BaseException(string message) : base(message) { }

        public BaseException(string message, Exception innerException) : base(message, innerException) { }

        protected BaseException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
