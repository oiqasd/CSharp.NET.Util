using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CSharp.Net.Util.Cryptography
{
    /// <summary>
    /// 数据加密算法 Data Encryption Algorithm
    /// </summary>
    public class DES
    {
        //默认密钥向量
        private static byte[] KeysIV = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="encryptString"></param>
        /// <returns></returns>
        public static string Encrypt(string encryptString)
        {
            return Encrypt(encryptString, Encoding.UTF8.GetString(KeysIV));
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="decryptString"></param>
        /// <returns></returns>
        public static string Decrypt(string decryptString)
        {
            return Decrypt(decryptString, Encoding.UTF8.GetString(KeysIV));
        }

        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="encryptString">待加密的字符串</param>
        /// <param name="encryptKey">密匙</param>
        /// <returns></returns>
        public static string Encrypt(string encryptString, string encryptKey)
        {
            //DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            using (var des = System.Security.Cryptography.DES.Create())
            {
                byte[] inputByteArray = Encoding.UTF8.GetBytes(encryptString);
                des.Key = Encoding.UTF8.GetBytes(encryptKey);
                des.IV = KeysIV;// Encoding.UTF8.GetBytes(encryptKey);// 初始化向量
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="decryptString">待解密的字符串</param>
        /// <param name="decryptKey">密匙</param>
        /// <returns></returns>
        public static string Decrypt(string decryptString, string decryptKey)
        {
            try
            {
                //DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                using (var des = System.Security.Cryptography.DES.Create())
                {
                    byte[] inputByteArray = Convert.FromBase64String(decryptString);
                    des.Key = Encoding.UTF8.GetBytes(decryptKey);
                    des.IV = KeysIV;
                    MemoryStream ms = new MemoryStream();
                    CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    // 如果两次密匙不一样，这一步可能会引发异常
                    cs.FlushFinalBlock();
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            catch
            {
                return "";
            }
        }

    }
}
