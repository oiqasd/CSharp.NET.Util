// ****************************************************
// * 创建日期：
// * 创建人：
// * 备注：
// ****************************************************

using CSharp.Net.Util;

namespace CSharp.Net.Mvc
{
    public class RequestExpiredException : BaseErrorException
    {
        public RequestExpiredException(string message="请求已过期") : base(message) { }
    }
}
