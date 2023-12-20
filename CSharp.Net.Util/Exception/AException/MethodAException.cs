using System.Collections.Generic;
using System.Reflection;

namespace CSharp.Net.Util.AException
{
    internal class MethodAException
    {
        /// <summary>
        /// 出异常的方法
        /// </summary>
        public MethodBase ErrorMethod { get; set; }

        /// <summary>
        /// 异常特性
        /// </summary>
        public IEnumerable<AExceptionAttribute> AExceptionAttributes { get; set; }
    }
}
