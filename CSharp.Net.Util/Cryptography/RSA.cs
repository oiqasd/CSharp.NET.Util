#define WinX
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace CSharp.Net.Util.Cryptography
{
    /// <summary>
    /// RSA加密类
    /// 需要使用pkcs#1标准生成的keys
    /// </summary>
    public sealed class RSA
    {
        #region 签名
        /// <summary>
        /// 使用私钥签名
        /// </summary>
        /// <param name="data"></param>
        /// <param name="privateKey"></param>
        /// <param name="rsaType"></param>
        /// <param name="charset"></param>
        /// <returns></returns>
        public static string Sign(string data, string privateKey, RSAType rsaType = RSAType.MD5, string charset = "utf-8")
        {
            if (string.IsNullOrEmpty(privateKey))
            {
                throw new Exception("Private key can't be null.");
            }
            byte[] bt = Encoding.GetEncoding(charset).GetBytes(data);

            using (System.Security.Cryptography.RSA rsa = CreateRsaProviderFromPrivateKey(privateKey))
            {
                if (rsa == null) throw new Exception("Private key is error");
                var signatureBytes = rsa.SignData(bt, DicAlgorithmName[rsaType], RSASignaturePadding.Pkcs1);
                return Convert.ToBase64String(signatureBytes);
            }


        }


        /// <summary>
        /// 使用公钥验证签名
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <param name="sign">签名</param>
        /// <returns></returns>
        public static bool CheckSign(string data, string sign, string publicKey, RSAType rsaType = RSAType.MD5, string charset = "utf-8")
        {
            if (string.IsNullOrEmpty(publicKey))
            {
                throw new Exception("Public key can't be null.");
            }
            byte[] bt = Encoding.GetEncoding(charset).GetBytes(data);
            byte[] signBytes = Convert.FromBase64String(sign);

            using (System.Security.Cryptography.RSA rsa = CreateRsaProviderFromPublicKey(publicKey))
            {
                if (rsa == null) throw new Exception("Public key is error");
                var verify = rsa.VerifyData(bt, signBytes, DicAlgorithmName[rsaType], RSASignaturePadding.Pkcs1);
                return verify;
            }

        }

        #endregion

        #region 解密

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="decryptData"></param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public static string Decrypt(string decryptData, string privateKey, string charset = "utf-8")
        {
            using (var rsa = CreateRsaProviderFromPrivateKey(privateKey))
            {
                return Encoding.GetEncoding(charset).GetString(rsa.Decrypt(Convert.FromBase64String(decryptData), RSAEncryptionPadding.Pkcs1));
            }
        }

        #endregion

        #region 加密

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Encrypt(string text, string publicKey, string charset = "utf-8")
        {
            using (var rsa = CreateRsaProviderFromPublicKey(publicKey))
            {
                var data = rsa.Encrypt(Encoding.GetEncoding(charset).GetBytes(text), RSAEncryptionPadding.Pkcs1);
                return Convert.ToBase64String(data);
            }

        }

        #endregion

        #region 创建RSA实例

        /// <summary>
        /// 使用私钥创建RSA实例
        /// </summary>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        private static System.Security.Cryptography.RSA CreateRsaProviderFromPrivateKey(string privateKey)
        {
            var privateKeyBits = Convert.FromBase64String(privateKey);

            var rsa = System.Security.Cryptography.RSA.Create();
            var rsaParameters = new RSAParameters();

            try
            {
                using (BinaryReader binr = new BinaryReader(new MemoryStream(privateKeyBits)))
                {
                    byte bt = 0;
                    ushort twobytes = 0;
                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130)
                        binr.ReadByte();
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();
                    else
                        throw new Exception("Unexpected value read binr.ReadUInt16()");

                    //bt = binr.ReadByte();
                    //if (bt != 0x02)
                    //    throw new Exception("Unexpected value read binr.ReadByte()");

                    twobytes = binr.ReadUInt16();
                    if (twobytes != 0x0102)
                        throw new Exception("Unexpected version");

                    bt = binr.ReadByte();
                    if (bt != 0x00)
                        throw new Exception("Unexpected value read binr.ReadByte()");

                    //------  all private key components are Integer sequences ----
                    rsaParameters.Modulus = binr.ReadBytes(GetIntegerSize(binr));
                    rsaParameters.Exponent = binr.ReadBytes(GetIntegerSize(binr));
                    rsaParameters.D = binr.ReadBytes(GetIntegerSize(binr));
                    rsaParameters.P = binr.ReadBytes(GetIntegerSize(binr));
                    rsaParameters.Q = binr.ReadBytes(GetIntegerSize(binr));
                    rsaParameters.DP = binr.ReadBytes(GetIntegerSize(binr));
                    rsaParameters.DQ = binr.ReadBytes(GetIntegerSize(binr));
                    rsaParameters.InverseQ = binr.ReadBytes(GetIntegerSize(binr));
                }

                rsa.ImportParameters(rsaParameters);
                return rsa;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 使用公钥创建RSA实例
        /// </summary>
        /// <param name="publicKeyString"></param>
        /// <returns></returns>
        private static System.Security.Cryptography.RSA CreateRsaProviderFromPublicKey(string publicKeyString)
        {
            // encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
            byte[] seqOid = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] seq = new byte[15];

            var x509Key = Convert.FromBase64String(publicKeyString);

            // ---------  Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob  ------
            using (MemoryStream mem = new MemoryStream(x509Key))
            {
                using (BinaryReader binr = new BinaryReader(mem))  //wrap Memory Stream with BinaryReader for easy reading
                {
                    byte bt = 0;
                    ushort twobytes = 0;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();   //advance 2 bytes
                    else
                        return null;

                    seq = binr.ReadBytes(15);       //read the Sequence OID
                    if (!CompareBytearrays(seq, seqOid))    //make sure Sequence for OID is correct
                        return null;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8103) //data read as little endian order (actual data order for Bit String is 03 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8203)
                        binr.ReadInt16();   //advance 2 bytes
                    else
                        return null;

                    bt = binr.ReadByte();
                    if (bt != 0x00)     //expect null byte next
                        return null;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();   //advance 2 bytes
                    else
                        return null;

                    twobytes = binr.ReadUInt16();
                    byte lowbyte = 0x00;
                    byte highbyte = 0x00;

                    if (twobytes == 0x8102) //data read as little endian order (actual data order for Integer is 02 81)
                        lowbyte = binr.ReadByte();  // read next bytes which is bytes in modulus
                    else if (twobytes == 0x8202)
                    {
                        highbyte = binr.ReadByte(); //advance 2 bytes
                        lowbyte = binr.ReadByte();
                    }
                    else
                        return null;
                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };   //reverse byte order since asn.1 key uses big endian order
                    int modsize = BitConverter.ToInt32(modint, 0);

                    int firstbyte = binr.PeekChar();
                    if (firstbyte == 0x00)
                    {   //if first byte (highest order) of modulus is zero, don't include it
                        binr.ReadByte();    //skip this null byte
                        modsize -= 1;   //reduce modulus buffer size by 1
                    }

                    byte[] modulus = binr.ReadBytes(modsize);   //read the modulus bytes

                    if (binr.ReadByte() != 0x02)            //expect an Integer for the exponent data
                        return null;
                    int expbytes = (int)binr.ReadByte();        // should only need one byte for actual exponent data (for all useful values)
                    byte[] exponent = binr.ReadBytes(expbytes);

                    // ------- create RSACryptoServiceProvider instance and initialize with public key -----
                    var rsa = System.Security.Cryptography.RSA.Create();
                    RSAParameters rsaKeyInfo = new RSAParameters
                    {
                        Modulus = modulus,
                        Exponent = exponent
                    };
                    rsa.ImportParameters(rsaKeyInfo);

                    return rsa;
                }

            }
        }

        #endregion

        #region 创建私钥公钥

        /// <summary>
        /// 创建私钥公钥
        /// </summary>
        /// <returns>1：私钥；2：公钥</returns>
        public (string, string) CreateKey()
        {
            string pubKey = string.Empty;
            string priKey = string.Empty;


#if false // UNITY_EDITOR_WIN||UNITY_STANDALONE_WIN||WinX||NETSTANDARD2_0        
            using (var rsa = new RSACryptoServiceProvider())
#else
            using (var rsa = System.Security.Cryptography.RSA.Create())
#endif
            {
#if NETSTANDARD2_0 
                priKey = ExportKeyToPEMFormat(rsa, true);
                pubKey = ExportKeyToPEMFormat(rsa, false);
#else
         
                priKey = rsa.ToXmlString(true);
                pubKey = rsa.ToXmlString(false);
#endif
                return (priKey, pubKey);
            }
        }


        private static string ExportKeyToPEMFormat(System.Security.Cryptography.RSA rsa, bool privateKey)
        {
            TextWriter outputStream = new StringWriter();

            var parameters = rsa.ExportParameters(privateKey);

            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);
                #region test 2019-1-17

                writer.Write((ushort)0x8130);
                writer.Write((byte)0x00);

                #endregion


                using (var innerStream = new MemoryStream())
                {
                    var innerWriter = new BinaryWriter(innerStream);
                    //EncodeIntegerBigEndian(innerWriter, new byte[] { 0x00 }); // Version

                    if (privateKey)
                    {
                        writer.Write((ushort)0x0102);
                        writer.Write((byte)0x00);

                        EncodeIntegerBigEndian(innerWriter, parameters.Modulus);
                        EncodeIntegerBigEndian(innerWriter, parameters.Exponent);
                        EncodeIntegerBigEndian(innerWriter, parameters.D);
                        EncodeIntegerBigEndian(innerWriter, parameters.P);
                        EncodeIntegerBigEndian(innerWriter, parameters.Q);
                        EncodeIntegerBigEndian(innerWriter, parameters.DP);
                        EncodeIntegerBigEndian(innerWriter, parameters.DQ);
                        EncodeIntegerBigEndian(innerWriter, parameters.InverseQ);

                        var length = (int)innerStream.Length;
                        // EncodeLength(writer, length); //del 2019-1-18
                        writer.Write(innerStream.GetBuffer(), 0, length);
                    }
                    else
                    {
                        #region 公钥
                        #region 2019-1-19
                        byte[] seqOid = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };

                        foreach (var b in seqOid)
                            writer.Write(b);

                        writer.Write((ushort)0x8103);
                        writer.Write((byte)0x00);
                        writer.Write((byte)0x00);

                        writer.Write((ushort)0x8130);
                        writer.Write((byte)0x00);

                        //data read as little endian order (actual data order for Integer is 02 81)
                        writer.Write((ushort)0x8102);

                        throw new Exception("写不出来了 2019/1/19 23：06");


                        #endregion

                        #endregion

                    }

                }

                var base64 = Convert.ToBase64String(stream.GetBuffer(), 0, (int)stream.Length).ToCharArray();
                outputStream.WriteLine($"-----BEGIN {(privateKey ? "PRIVATE" : "PUBLIC")} KEY-----");
                // Output as Base64 with lines chopped at 64 characters
                for (var i = 0; i < base64.Length; i += 64)
                {
                    outputStream.Write(base64, i, Math.Min(64, base64.Length - i));
                }
                outputStream.WriteLine($"-----END {(privateKey ? "PRIVATE" : "PUBLIC")} KEY-----");

                return outputStream.ToString();

            }
        }

        #region XML转换
        public static string ToXmlStirng(System.Security.Cryptography.RSA rsa, bool privateKey)
        {
            RSAParameters parameters = rsa.ExportParameters(privateKey);

            return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent><D>{2}</D><P>{3}</P><Q>{4}</Q><DP>{5}</DP><DQ>{6}</DQ><InverseQ>{7}</InverseQ></RSAKeyValue>",
                 parameters.Modulus != null ? Convert.ToBase64String(parameters.Modulus) : null,
                 parameters.Exponent != null ? Convert.ToBase64String(parameters.Exponent) : null,
                 parameters.D != null ? Convert.ToBase64String(parameters.D) : null,
                 parameters.P != null ? Convert.ToBase64String(parameters.P) : null,
                 parameters.Q != null ? Convert.ToBase64String(parameters.Q) : null,
                 parameters.DP != null ? Convert.ToBase64String(parameters.DP) : null,
                 parameters.DQ != null ? Convert.ToBase64String(parameters.DQ) : null,
                 parameters.InverseQ != null ? Convert.ToBase64String(parameters.InverseQ) : null);
        }

        public static void FromXmlString(System.Security.Cryptography.RSA rsa, string xmlString)
        {
            RSAParameters parameters = new RSAParameters();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            if (xmlDoc.DocumentElement.Name.Equals("RSAKeyValue"))
            {
                foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "Modulus": parameters.Modulus = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "Exponent": parameters.Exponent = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "D": parameters.D = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "P": parameters.P = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "Q": parameters.Q = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "DP": parameters.DP = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "DQ": parameters.DQ = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "InverseQ": parameters.InverseQ = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                    }
                }
            }
            else
            {
                throw new Exception("Invalid XML RSA key.");
            }

            rsa.ImportParameters(parameters);
        }
        #endregion

        #region 生成通用私钥
        /// <summary>
        /// 生成通用私钥
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value"></param>
        /// <param name="forceUnsigned"></param>
        private static void EncodeIntegerBigEndian(BinaryWriter stream, byte[] value, bool forceUnsigned = true)
        {
            stream.Write((byte)0x02); // INTEGER 

            var prefixZeros = 0;
            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] != 0) break;
                prefixZeros++;
            }
            if (value.Length - prefixZeros == 0)
            {
                EncodeLength(stream, 1);
                stream.Write((byte)0);
            }
            else
            {
                if (forceUnsigned && value[prefixZeros] > 0x7f)
                {
                    // Add a prefix zero to force unsigned if the MSB is 1
                    EncodeLength(stream, value.Length - prefixZeros + 1);
                    stream.Write((byte)0);
                }
                else
                {
                    EncodeLength(stream, value.Length - prefixZeros);
                }
                for (var i = prefixZeros; i < value.Length; i++)
                {
                    stream.Write(value[i]);
                }
            }
        }

        private static void EncodeLength(BinaryWriter stream, int length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException("length", "Length must be non-negative");
            if (length < 0x80)
            {
                // Short form
                stream.Write((byte)length);
            }
            else
            {
                // Long form
                var temp = length;
                var bytesRequired = 0;
                while (temp > 0)
                {
                    temp >>= 8;
                    bytesRequired++;
                }
                stream.Write((byte)(bytesRequired | 0x80));
                for (var i = bytesRequired - 1; i >= 0; i--)
                {
                    stream.Write((byte)(length >> (8 * i) & 0xff));
                }
            }
        }
        #endregion

        #endregion

        #region 导入密钥算法

        /// <summary>
        /// 计算私钥节点长度
        /// </summary>
        /// <param name="binr"></param>
        /// <returns></returns>
        private static int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02) //expect integer
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
                count = binr.ReadByte(); // data size in next byte
            else if (bt == 0x82)
            {
                var highbyte = binr.ReadByte(); // data size in next 2 bytes
                var lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;  // we already have the data size
            }

            while (binr.ReadByte() == 0x00)
            {
                count -= 1;//remove high order zeros in data
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current); //last ReadByte wasn't a removed zero, so back up a byte
            return count;
        }


        /// <summary>
        /// 比较OID
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static bool CompareBytearrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            int i = 0;
            foreach (byte c in a)
            {
                if (c != b[i])
                    return false;
                i++;
            }
            return true;
        }

        /// <summary>
        /// RSA算法类型字典
        /// </summary>
        private static SortedDictionary<RSAType, HashAlgorithmName> DicAlgorithmName = new SortedDictionary<RSAType, HashAlgorithmName>()
        {
            { RSAType.MD5, HashAlgorithmName.MD5 },
            { RSAType.SHA1, HashAlgorithmName.SHA1 },
            { RSAType.SHA256, HashAlgorithmName.SHA256 },
            { RSAType.SHA512, HashAlgorithmName.SHA512 }
        };
        #endregion
    }

    /// <summary>
    /// RSA算法类型
    /// </summary>
    public enum RSAType
    {
        /// <summary>
        /// MD5
        /// </summary>
        MD5,

        /// <summary>
        /// SHA1
        /// </summary>
        SHA1,

        /// <summary>
        /// SHA256 密钥长度至少为2048 
        /// </summary>
        SHA256,

        /// <summary>
        /// SHA512
        /// </summary>
        SHA512
    }
}
