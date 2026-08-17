using System;
namespace CSharp.Net.Util
{
    public class Args
    {
        public static void Verify(bool val, string message = "参数错误")
        {
            if (!val)
            {
                throw new ArgumentException(message);
            }
        }
    }
}
