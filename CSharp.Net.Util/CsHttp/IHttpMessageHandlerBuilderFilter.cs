using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp.Net.Util.CsHttp
{
    interface IHttpMessageHandlerBuilderFilter
    {
        //
        // 摘要:
        //     Applies additional initialization to the Microsoft.Extensions.Http.HttpMessageHandlerBuilder
        //
        //
        // 参数:
        //   next:
        //     A delegate which will run the next Microsoft.Extensions.Http.IHttpMessageHandlerBuilderFilter.
        Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next);
    }
}
