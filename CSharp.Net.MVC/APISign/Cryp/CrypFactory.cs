// ****************************************************
// * 创建日期：
// * 创建人：
// * 备注：
// ****************************************************

namespace CSharp.Net.Mvc.ApiSign.Cryp
{
    internal class CrypFactory
    {
        public static ICryp SetCryp(string crypType="md5")
        {
            switch (crypType?.ToLower())
            {
                case "md5":
                    return new MD5Cryp();

                default:
                    return new MD5Cryp();
            }
        }

        
    }
}
