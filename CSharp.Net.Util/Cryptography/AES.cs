using System;
using System.Security.Cryptography;
using System.Text;

namespace CSharp.Net.Util.Cryptography
{
    /// <summary>
    /// 对称加密
    /// </summary>
    public class AES
    {
        /// <summary>
        /// 有密码的AES加密 
        /// </summary>
        /// <param name="sourceData">原文</param>
        /// <param name="key">密钥,32位不足自动补位,超出截取,默认:<![CDATA[Buz(%&hj7x89H$yupI0h56FtEaT5&fvCUFgy76*h%(HilJ$lhj!y6&(*jkP87jH7]]></param>
        /// <param name="iv">iv,16位自动补位,默认:<![CDATA[Z4ghj*BIg7!rNIfb&96GUY82GfghUb#er57HBh(u%g6HJ($jhWk7&!hg1ui%$hjk]]></param>
        /// <returns></returns>
        public static string Encrypt(string sourceData, string key = null, string iv = null)
        {
            byte[] toEncryptArray = Encoding.UTF8.GetBytes(sourceData);
            SymmetricAlgorithm symmetric = Aes.Create();//new RijndaelManaged();
            symmetric.Key = GetKey(symmetric, key);
            symmetric.IV = GetIV(symmetric, iv);
            symmetric.Mode = CipherMode.CBC;
            symmetric.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = symmetric.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            //MemoryStream ms = new MemoryStream();
            //CryptoStream cs = new CryptoStream(ms, cTransform, CryptoStreamMode.Write);
            //cs.Write(toEncryptArray, 0, toEncryptArray.Length);
            //cs.FlushFinalBlock();
            //ms.Close();
            //byte[] bytOut = ms.ToArray();
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="encryptData">密文</param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string Decrypt(string encryptData, string key = null, string iv = null)
        {
            byte[] keyArray = Encoding.UTF8.GetBytes(key);
            byte[] toEncryptArray = Convert.FromBase64String(encryptData);
            SymmetricAlgorithm symmetric = Aes.Create();
            symmetric.Key = keyArray;
            symmetric.Mode = CipherMode.CBC;
            symmetric.Padding = PaddingMode.PKCS7;
            symmetric.IV = GetIV(symmetric, iv);
            ICryptoTransform cTransform = symmetric.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            //CryptoStream cs = new CryptoStream(ms, cTransform, CryptoStreamMode.Read);
            //StreamReader sr = new StreamReader(cs);
            //return sr.ReadToEnd();
            return Encoding.UTF8.GetString(resultArray);
        }

        /// <summary>
        /// 随机生成32位AESkey
        /// </summary>
        /// <returns></returns>
        public static string GenerateAESKey(int length = 32)
        {
            string str = string.Empty;

            Random rnd1 = new Random();
            int r = rnd1.Next(10, 100);

            long num2 = DateTime.Now.Ticks + r;

            Random random = new Random(((int)(((ulong)num2) & 0xffffffffL)) | ((int)(num2 >> r)));
            for (int i = 0; i < 32; i++)
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

        /// <summary>   
        /// 获得初始向量IV   
        /// </summary>   
        /// <returns>初试向量IV</returns>   
        private static byte[] GetIV(SymmetricAlgorithm symmetricAlgorithm, string iv)
        {
            string sTemp = iv.IsNotNullOrEmpty() ? iv : "Z4ghj*BIg7!rNIfb&96GUY82GfghUb#er57HBh(u%g6HJ($jhWk7&!hg1ui%$hjk";
            symmetricAlgorithm.GenerateIV();
            byte[] bytTemp = symmetricAlgorithm.IV;
            int IVLength = bytTemp.Length;
            if (sTemp.Length > IVLength)
                sTemp = sTemp.Substring(0, IVLength);
            else if (sTemp.Length < IVLength)
                sTemp = sTemp.PadRight(IVLength, ' ');
            return Encoding.UTF8.GetBytes(sTemp);
        }

        /// <summary>   
        /// 获得密钥   
        /// </summary>   
        /// <returns>密钥</returns>   
        private static byte[] GetKey(SymmetricAlgorithm symmetricAlgorithm, string key)
        {
            string sTemp = key.IsNotNullOrEmpty() ? key : "Buz(%&hj7x89H$yupI0h56FtEaT5&fvCUFgy76*h%(HilJ$lhj!y6&(*jkP87jH7";
            symmetricAlgorithm.GenerateKey();
            byte[] bytTemp = symmetricAlgorithm.Key;
            int KeyLength = bytTemp.Length;
            if (sTemp.Length > KeyLength)
                sTemp = sTemp.Substring(0, KeyLength);
            else if (sTemp.Length < KeyLength)
                sTemp = sTemp.PadRight(KeyLength, ' ');
            return Encoding.UTF8.GetBytes(sTemp);
        }
    }
}
