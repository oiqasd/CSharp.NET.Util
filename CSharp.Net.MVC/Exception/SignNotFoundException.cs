namespace CSharp.Net.Mvc;

public class SignNotFoundException : ErrorException
{
    public SignNotFoundException(string message = "签名信息不存在")
        : base(ReturnCode.CheckSignError, message)
    {
    }
}
