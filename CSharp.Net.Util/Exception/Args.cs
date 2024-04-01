using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp.Net.Util
{
    public class Args
    {
        public static void Verify(bool val, string message = "参数错误")
        {
            if (!val)
            {
                throw new ArgsException(message);
            }
        }
    }
}
