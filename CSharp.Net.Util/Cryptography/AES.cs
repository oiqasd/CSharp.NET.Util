using System;
using System.Security.Cryptography;
using System.Text;

namespace CSharp.Net.Util.Cryptography
{
    /// <summary>
    /// 高级加密标准 Advanced Encryption Standard
    /// Rijndael加密法
    /// </summary>
    public class AES
    {
        /// <summary>
        /// 有密码的AES加密 
        /// </summary>
        /// <param name="text">加密字符</param>
        /// <param name="key">加密的密码</param>
        /// <returns></returns>
        public static string Encrypt(string text, string key)
        {
            byte[] keyArray = Encoding.UTF8.GetBytes(key);
            byte[] toEncryptArray = Encoding.UTF8.GetBytes(text);

            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = rDel.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string Decrypt(string text, string key)
        {
            byte[] keyArray = Encoding.UTF8.GetBytes(key);
            byte[] toEncryptArray = Convert.FromBase64String(text);

            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = rDel.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Encoding.UTF8.GetString(resultArray);
        }

        /// <summary>
        /// 随机生成16位AESkey
        /// </summary>
        /// <returns></returns>
        public static string GenerateAESKey()
        {
            string str = string.Empty;

            Random rnd1 = new Random();
            int r = rnd1.Next(10, 100);

            long num2 = DateTime.Now.Ticks + r;

            Random random = new Random(((int)(((ulong)num2) & 0xffffffffL)) | ((int)(num2 >> r)));
            for (int i = 0; i < 16; i++)
            {
                char ch;
                int num = random.Next();
                if ((num % 2) == 0)
                {
                    ch = (char)(0x30 + ((ushort)(num % 10)));
                }
                else
                {
                    ch = (char)(0x41 + ((ushort)(num % 0x1a)));
                }
                str += ch.ToString();
            }
            return str;
        }
    }
}
