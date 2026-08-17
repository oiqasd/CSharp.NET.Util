using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp.Net.Mvc
{
    /// <summary>
    /// 请求对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Request<T> :BaseRequest
    {
        public Request()
        {
            Parameter = default(T);
        }
        /// <summary>
        /// 传入参数
        /// </summary>
        public T Parameter { get; set; } 
    }
}

