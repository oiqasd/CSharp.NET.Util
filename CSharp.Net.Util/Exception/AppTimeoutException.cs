using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp.Net.Util
{
    public class AppTimeoutException : AppException
    {
        public AppTimeoutException() : base(408, "Timeout") { }
        public AppTimeoutException(string message) : base(408, message) { }
    }
}
