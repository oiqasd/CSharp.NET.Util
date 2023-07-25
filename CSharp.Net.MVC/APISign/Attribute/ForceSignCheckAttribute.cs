// ****************************************************
// * 创建日期：
// * 创建人：
// * 备注：
// ****************************************************

using System;

namespace CSharp.Net.Mvc
{
    /// <summary>
    /// 强制校验签名
    /// 默认[AllowAnonymous]、[IngoreSignCheck]及[IgnoreMethod]不进行校验
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true)]
    public class ForceSignCheckAttribute : Attribute
    {
    }
}
