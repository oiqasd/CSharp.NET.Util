// ****************************************************
// * 创建日期：2023-7-17
// * 创建人：y
// * 备注：
// ****************************************************

using CSharp.Net.Util;

namespace CSharp.Net.Mvc;

public class SignErrorException : BaseException
{
    public SignErrorException(string message) : base(message) { }
}
