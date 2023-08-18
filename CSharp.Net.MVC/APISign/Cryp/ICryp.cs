// ****************************************************
// * 创建日期：
// * 创建人：
// * 备注：
// ****************************************************

namespace CSharp.Net.Mvc.ApiSign.Cryp;

internal interface ICryp
{
    void Check();

    string Encrypt(string data,string appKey);
}
