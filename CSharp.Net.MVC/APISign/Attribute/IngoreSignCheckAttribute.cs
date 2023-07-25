// ****************************************************
// * 创建日期：
// * 创建人：
// * 备注：
// ****************************************************

using System;

namespace CSharp.Net.Mvc
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true)]
    public class IngoreSignCheckAttribute : Attribute
    {
    }
}
