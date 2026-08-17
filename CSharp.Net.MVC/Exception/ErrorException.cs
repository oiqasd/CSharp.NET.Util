using System;

namespace CSharp.Net.Mvc;

public class ErrorException : Exception
{
    public ErrorException(ReturnCode code)
        : this(code, code.GetDescription())
    {
    }

    public ErrorException(string message)
        : this(ReturnCode.DataError, message)
    {
    }

    public ErrorException(
        ReturnCode code,
        string message,
        object responseData = null,
        Exception innerException = null)
        : base(message, innerException)
    {
        Code = code;
        ResponseData = responseData;
    }

    public ReturnCode Code { get; }

    public object ResponseData { get; }
}
