using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CSharp.Net.Util.Cryptography
{
    /// <summary>
    /// 对称加密 
    /// Advanced Encryption Standard，高级加密标准
    /// </summary>
    public class AES
    {
        // AES算法支持的密钥长度：128位、192位、256位
        // 向量长度固定为128位(16字节)
        private const int KeySize = 256;
        private const int BlockSize = 128;

        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="key">密钥(32字节 for 256位, 24字节 for 192位, 16字节 for 128位),base64字符串</param>
        /// <returns>加密后的字符串(包含向量)</returns>
        public static string Encrypt(string plainText, string key = "")
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException(nameof(plainText));

            if (string.IsNullOrEmpty(key))
                key = AesDefaultKey;
            //throw new ArgumentNullException(nameof(key));

            // 将密钥转换为字节数组并验证长度
            byte[] keyBytes = Convert.FromBase64String(key);
            ValidateKeyLength(keyBytes);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.KeySize = KeySize;
                aesAlg.BlockSize = BlockSize;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                // 生成随机向量
                aesAlg.GenerateIV();
                byte[] iv = aesAlg.IV;

                // 创建加密器
                using ICryptoTransform encryptor = aesAlg.CreateEncryptor(keyBytes, iv);

                // 加密过程
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    // 先写入向量，解密时需要用到
                    msEncrypt.Write(iv, 0, iv.Length);

                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        // 将明文写入加密流
                        swEncrypt.Write(plainText);
                        swEncrypt.Flush();
                        csEncrypt.FlushFinalBlock();
                    }

                    // 转换为Base64字符串返回(包含向量)
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="cipherText">加密后的字符串(包含向量)</param>
        /// <param name="key">密钥(与加密时使用的密钥相同)</param>
        /// <returns>解密后的明文</returns>
        public static string Decrypt(string cipherText, string key = "")
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException(nameof(cipherText));

            if (string.IsNullOrEmpty(key))
                key = AesDefaultKey;

            // 将密钥转换为字节数组并验证长度
            byte[] keyBytes = Convert.FromBase64String(key);
            ValidateKeyLength(keyBytes);

            // 将加密字符串转换为字节数组
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.KeySize = KeySize;
                aesAlg.BlockSize = BlockSize;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                // 从加密数据中提取向量(前16字节)
                byte[] iv = new byte[BlockSize / 8];
                Array.Copy(cipherBytes, 0, iv, 0, iv.Length);
                aesAlg.IV = iv;

                // 创建解密器
                using ICryptoTransform decryptor = aesAlg.CreateDecryptor(keyBytes, aesAlg.IV);

                // 解密过程
                using (MemoryStream msDecrypt = new MemoryStream(cipherBytes, iv.Length, cipherBytes.Length - iv.Length))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                {
                    // 从解密流中读取明文
                    return srDecrypt.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// 生成符合AES要求的随机密钥
        /// </summary>        
        /// <returns>随机密钥字符串</returns>    
        public static string GenerateRandomKey()
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.KeySize = KeySize;
                aesAlg.GenerateKey();
                return Convert.ToBase64String(aesAlg.Key);
            }
        }

        /// <summary>
        /// 验证密钥长度是否符合要求
        /// </summary>
        private static void ValidateKeyLength(byte[] keyBytes)
        {
            int validKeySize = KeySize / 8; // 转换为字节数
            if (keyBytes.Length != validKeySize)
            {
                throw new ArgumentException($"密钥长度必须为{validKeySize}字节", nameof(keyBytes));
            }
        }

        /// <summary>   
        /// 获得密钥   
        /// </summary>   
        /// <returns>密钥</returns>   
        static readonly string AesDefaultKey = "ibzdO1JTUrQGyZ3rNGTsnGxd7O0A350ScGjBzWfYfbw=";
    }
}
