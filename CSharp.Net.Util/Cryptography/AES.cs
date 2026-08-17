using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CSharp.Net.Util.Cryptography
{
    /// <summary>
    /// AES 对称加密。现代目标框架写入 AES-GCM 格式，并兼容读取旧 CBC 格式。
    /// </summary>
    public class AES
    {
        private const int KeySize = 256;
        private const int BlockSize = 128;
        private const string GcmPrefix = "aes-gcm:v1:";
        private const int GcmNonceSize = 12;
        private const int GcmTagSize = 16;
        private static readonly string AesDefaultKey = "ibzdO1JTUrQGyZ3rNGTsnGxd7O0A350ScGjBzWfYfbw=";

        public static string Encrypt(string plainText, string key = "")
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException(nameof(plainText));

            byte[] keyBytes = GetKeyBytes(key);
#if NET
            byte[] nonce = new byte[GcmNonceSize];
            RandomNumberGenerator.Fill(nonce);
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = new byte[plainBytes.Length];
            byte[] tag = new byte[GcmTagSize];

            using (var aes = new AesGcm(keyBytes, GcmTagSize))
                aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

            byte[] payload = new byte[nonce.Length + tag.Length + cipherBytes.Length];
            Buffer.BlockCopy(nonce, 0, payload, 0, nonce.Length);
            Buffer.BlockCopy(tag, 0, payload, nonce.Length, tag.Length);
            Buffer.BlockCopy(cipherBytes, 0, payload, nonce.Length + tag.Length, cipherBytes.Length);
            return GcmPrefix + Convert.ToBase64String(payload);
#else
            return EncryptLegacy(plainText, keyBytes);
#endif
        }

        public static string Decrypt(string cipherText, string key = "")
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException(nameof(cipherText));

            byte[] keyBytes = GetKeyBytes(key);
            if (!cipherText.StartsWith(GcmPrefix, StringComparison.Ordinal))
                return DecryptLegacy(cipherText, keyBytes);

#if NET
            byte[] payload = Convert.FromBase64String(cipherText.Substring(GcmPrefix.Length));
            if (payload.Length < GcmNonceSize + GcmTagSize)
                throw new CryptographicException("Invalid AES-GCM payload.");

            byte[] nonce = new byte[GcmNonceSize];
            byte[] tag = new byte[GcmTagSize];
            byte[] encrypted = new byte[payload.Length - GcmNonceSize - GcmTagSize];
            Buffer.BlockCopy(payload, 0, nonce, 0, nonce.Length);
            Buffer.BlockCopy(payload, nonce.Length, tag, 0, tag.Length);
            Buffer.BlockCopy(payload, nonce.Length + tag.Length, encrypted, 0, encrypted.Length);

            byte[] plainBytes = new byte[encrypted.Length];
            using (var aes = new AesGcm(keyBytes, GcmTagSize))
                aes.Decrypt(nonce, encrypted, tag, plainBytes);
            return Encoding.UTF8.GetString(plainBytes);
#else
            throw new PlatformNotSupportedException("AES-GCM ciphertext requires a modern .NET target.");
#endif
        }

        public static string GenerateRandomKey()
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = KeySize;
                aes.GenerateKey();
                return Convert.ToBase64String(aes.Key);
            }
        }

        private static byte[] GetKeyBytes(string key)
        {
            byte[] keyBytes = Convert.FromBase64String(string.IsNullOrEmpty(key) ? AesDefaultKey : key);
            if (keyBytes.Length != KeySize / 8)
                throw new ArgumentException($"密钥长度必须为{KeySize / 8}字节", nameof(key));
            return keyBytes;
        }

        private static string EncryptLegacy(string plainText, byte[] keyBytes)
        {
            using (Aes aes = CreateLegacyAes(keyBytes))
            {
                aes.GenerateIV();
                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                using (var output = new MemoryStream())
                {
                    output.Write(aes.IV, 0, aes.IV.Length);
                    using (var crypto = new CryptoStream(output, encryptor, CryptoStreamMode.Write))
                    using (var writer = new StreamWriter(crypto, Encoding.UTF8))
                        writer.Write(plainText);
                    return Convert.ToBase64String(output.ToArray());
                }
            }
        }

        private static string DecryptLegacy(string cipherText, byte[] keyBytes)
        {
            byte[] payload = Convert.FromBase64String(cipherText);
            int ivLength = BlockSize / 8;
            if (payload.Length <= ivLength || (payload.Length - ivLength) % ivLength != 0)
                throw new CryptographicException("Invalid legacy AES-CBC payload.");

            byte[] iv = new byte[ivLength];
            Buffer.BlockCopy(payload, 0, iv, 0, iv.Length);
            using (Aes aes = CreateLegacyAes(keyBytes))
            using (ICryptoTransform decryptor = aes.CreateDecryptor(keyBytes, iv))
            using (var input = new MemoryStream(payload, iv.Length, payload.Length - iv.Length))
            using (var crypto = new CryptoStream(input, decryptor, CryptoStreamMode.Read))
            using (var reader = new StreamReader(crypto, Encoding.UTF8))
                return reader.ReadToEnd();
        }

        private static Aes CreateLegacyAes(byte[] keyBytes)
        {
            Aes aes = Aes.Create();
            aes.KeySize = KeySize;
            aes.BlockSize = BlockSize;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = keyBytes;
            return aes;
        }
    }
}
