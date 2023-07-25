// ****************************************************
// * 创建日期：
// * 创建人：y
// * 备注：
// ****************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CSharp.Net.Util
{
    public class BaseErrorException : Exception
    {
        public BaseErrorException()
        {
        }

        public BaseErrorException(string message) : base(message)
        {
        }

        public BaseErrorException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BaseErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
