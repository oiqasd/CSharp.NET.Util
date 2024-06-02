// ****************************************************
// * 创建日期：
// * 创建人：
// * 备注：
// ****************************************************

using CSharp.Net.Util.Cryptography;

namespace CSharp.Net.Mvc.ApiSign.Cryp;

internal class MD5Cryp : ICryp
{
    public void Check()
    {
        throw new NotImplementedException();
    }

    public string Encrypt(string data,string solt)
    {
       return MD5.Md5Encrypt16X(data+solt);
    }
}
