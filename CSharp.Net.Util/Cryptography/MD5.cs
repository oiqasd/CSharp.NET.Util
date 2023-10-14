using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CSharp.Net.Util.Cryptography
{
    /// <summary>
    /// md5相关
    /// </summary>
    public class MD5
    {
        /// <summary>
        /// 将字符串经过md5加密，返回加密后的字符串
        /// E10ADC3949BA59ABBE56E057F20F883E
        /// </summary>
        public static string Md5Encrypt(string strToBeEncrypt, string encoding = "utf-8")
        {
            Encoding encode = Encoding.GetEncoding(encoding);
            return Md5Encrypt(strToBeEncrypt, encode);
        }

        /// <summary>
        /// 获取16位长度
        /// </summary>
        /// <param name="strToBeEncrypt"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string Md5Encrypt16X(string strToBeEncrypt, string encoding = "utf-8")
        {
            Encoding encode = Encoding.GetEncoding(encoding);
            var e = Md5Encrypt(strToBeEncrypt, encode);
            return e.Substring(8, 16);
        }

        /// <summary>
        /// 将字符串经过md5hash加密，返回加密后的字符串
        /// </summary>
        public static string Md5Encrypt(string strToBeEncrypt, Encoding encoding)
        {
            // MD5 md5 = new MD5CryptoServiceProvider();
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] fromData = encoding.GetBytes(strToBeEncrypt);
                byte[] targetData = md5.ComputeHash(fromData);
                //StringBuilder Byte2Builder = new StringBuilder();
                //for (int i = 0; i < TargetData.Length; i++)
                //{
                //    Byte2Builder.Append(TargetData[i].ToString("x2"));
                //}
                //return Byte2Builder.ToString().ToLower();

                var strResult = BitConverter.ToString(targetData);
                return strResult.Replace("-", "");
            }

        }


        /// <summary>
        /// 获取Base64String
        /// 4QrcOUm6Wau+VuBX8g+IPg==
        /// </summary>
        /// <param name="str"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string MD5Base64String(string str, string encoding = "utf-8")
        {
            Encoding encode = Encoding.GetEncoding(encoding);
            return MD5Base64String(str, encode);
        }

        /// <summary>
        /// 获取Base64String
        /// </summary>
        /// <param name="str"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string MD5Base64String(string str, Encoding encoding)
        {
            if (string.IsNullOrEmpty(str)) return str;
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var result = md5.ComputeHash(encoding.GetBytes(str));
                return Convert.ToBase64String(result);
            }
        }
    }
}
