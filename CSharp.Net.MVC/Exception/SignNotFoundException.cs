// ****************************************************
// * 创建日期：
// * 创建人：
// * 备注：
// ****************************************************

using CSharp.Net.Util;

namespace CSharp.Net.Mvc;

public class SignNotFoundException : AppException
{
    public SignNotFoundException(string message = "签名信息不存在") : base(message) { }
}
