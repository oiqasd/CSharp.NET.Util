using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CSharp.Net.Util.Cryptography
{
    /// <summary>
    /// HMAC加密 SHA256
    /// </summary>
    public class HMAC
    {
        /// <summary>
        /// HmacSHA256加密Base64返回
        /// </summary>
        /// <param name="data"></param>
        /// <param name="secret"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string HmacSHA256_Base64(string data, string secret = "", string encoding = "utf-8")
        {
            byte[] FromData = Encoding.GetEncoding(encoding).GetBytes(data);
            using (var hmac256 = new HMACSHA256(Encoding.Default.GetBytes(secret)))
            {
                return Convert.ToBase64String(hmac256.ComputeHash(FromData));
            }
        }
    }
}
