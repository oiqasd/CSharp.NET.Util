// ****************************************************
// * 创建日期：
// * 创建人：
// * 备注：
// ****************************************************

using CSharp.Net.Util;

namespace CSharp.Net.Mvc;

public class SignNotExistException : BaseException
{
    public SignNotExistException(string message = "签名信息不存在") : base(message) { }
}
