using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp.Net.Mvc
{
    /// <summary>
    /// 请求参数对象基类
    /// </summary>
    public class BaseRequest
    {
        /// <summary>
        /// API版本
        /// </summary>
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// 方法名
        /// </summary>
        public string Method { get; set; } = "";
        
    }
}

