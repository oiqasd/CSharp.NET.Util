using CSharp.Net.Mvc;
using CSharp.Net.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ErrorException : AppException
{
    public ErrorException(ReturnCode code) : base(code, code.GetDescription())
    {
    }
    public ErrorException(string message) : base(ReturnCode.DataError, message)
    {
    }


    public ErrorException(ReturnCode code, string message) : base(code, message)
    {
    }
}

