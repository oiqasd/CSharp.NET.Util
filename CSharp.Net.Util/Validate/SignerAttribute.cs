using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.Net.Util.Validate
{
    /// <summary>
    /// 签名特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SignerAttribute : Attribute { }
}
