using CSharp.Net.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp.Net.Mvc
{
    /// <summary>
    /// 请求对象（带分页）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RequestPaged<T> : BaseRequest
    {
        public RequestPaged()
        {
            Parameter = default(T);
        }
        /// <summary>
        /// 传入参数
        /// </summary>
        public T Parameter { get; set; }

        /// <summary>
        /// 分页相关参数
        /// </summary>
        public PageArgument Paging { get; set; }
    }
}

