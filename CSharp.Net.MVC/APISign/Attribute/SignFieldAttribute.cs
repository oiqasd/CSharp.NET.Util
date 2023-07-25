// ****************************************************
// * 创建日期：
// * 创建人：
// * 备注：
// ****************************************************

using System;

namespace CSharp.Net.Mvc
{
    [AttributeUsage(AttributeTargets.Field|AttributeTargets.Parameter| AttributeTargets.Property, Inherited = true)]
    public class SignFieldAttribute : Attribute
    {
    }
}
