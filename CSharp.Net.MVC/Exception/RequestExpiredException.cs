// ****************************************************
// * 创建日期：
// * 创建人：
// * 备注：
// ****************************************************

using CSharp.Net.Util;

namespace CSharp.Net.Mvc;

public class RequestExpiredException : BaseException
{
    public RequestExpiredException(string message="请求已过期") : base(message) { }
}
