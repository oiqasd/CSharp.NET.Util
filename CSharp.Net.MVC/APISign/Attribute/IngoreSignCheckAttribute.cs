// ****************************************************
// * 创建日期：
// * 创建人：
// * 备注：
// ****************************************************


namespace CSharp.Net.Mvc;

/// <summary>
/// 不校验签名特性
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true)]
public class IngoreSignCheckAttribute : Attribute { }
