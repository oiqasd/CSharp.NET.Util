using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp.Net.Util
{
    public class ArgsException : AppException
    {
        public ArgsException(string message) : base(400, message) { }
    }
}
