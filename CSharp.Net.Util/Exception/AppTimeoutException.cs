using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp.Net.Util
{
    public class AppTimeoutException : AppException
    {
        public AppTimeoutException() : base(AExceptionCode.TimeOut, "Timeout") { }
        public AppTimeoutException(string message) : base(AExceptionCode.TimeOut, message) { }
    }
}
