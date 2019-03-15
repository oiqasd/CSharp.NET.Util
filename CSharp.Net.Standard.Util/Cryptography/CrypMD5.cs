using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CSharp.Net.Standard.Util.Cryptography
{
    public class CrypMD5
    {
        /// <summary>
        /// 将字符串经过md5加密，返回加密后的字符串的小写表示
        /// </summary>
        public static string Md5Encrypt(string strToBeEncrypt, string encoding = "utf-8")
        {
            Encoding encode = Encoding.GetEncoding(encoding);
            return Md5Encrypt(strToBeEncrypt, encode);
        }

        /// <summary>
        /// 将字符串经过md5加密，返回加密后的字符串的小写表示
        /// </summary>
        public static string Md5Encrypt(string strToBeEncrypt, Encoding encoding)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] FromData = encoding.GetBytes(strToBeEncrypt);
            byte[] TargetData = md5.ComputeHash(FromData);
            StringBuilder Byte2Builder = new StringBuilder();
            for (int i = 0; i < TargetData.Length; i++)
            {
                Byte2Builder.Append(TargetData[i].ToString("x2"));
            }
            return Byte2Builder.ToString().ToLower();
        }
    }
}
