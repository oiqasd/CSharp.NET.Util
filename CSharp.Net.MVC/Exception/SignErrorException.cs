namespace CSharp.Net.Mvc;

public class SignErrorException : ErrorException
{
    public SignErrorException(string message)
        : base(ReturnCode.CheckSignError, message)
    {
    }
}
