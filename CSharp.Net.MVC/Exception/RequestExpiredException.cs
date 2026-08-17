namespace CSharp.Net.Mvc;

public class RequestExpiredException : ErrorException
{
    public RequestExpiredException(string message = "请求已过期")
        : base(ReturnCode.ExpiredRequest, message)
    {
    }
}
