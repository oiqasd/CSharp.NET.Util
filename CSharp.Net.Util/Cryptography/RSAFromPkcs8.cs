﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CSharp.Net.Util.Cryptography
{
    /// <summary>
    /// 类名：RSAFromPkcs8
    /// 功能：RSA解密、签名、验签
    /// 详细：该类对Java生成的密钥进行解密和签名以及验签专用类，不需要修改
    /// 版本：1.0
    /// 日期：2013-09-30
    /// 说明：
    /// 以下代码只是为了方便商户测试而提供的样例代码，商户可以根据自己网站的需要，按照技术文档编写,并非一定要使用该代码。
    /// </summary>
    public sealed class RSAFromPkcs8
    {
        /// <summary>
        /// 签名
        /// </summary>
        /// <param name="content">待签名字符串</param>
        /// <param name="privateKey">私钥</param>
        /// <param name="charset">编码格式</param>
        /// <returns>签名后字符串</returns>
        public static string Sign(string content, string privateKey, string charset = "utf-8")
        {
            byte[] bt = Encoding.GetEncoding(charset).GetBytes(content);
            RSACryptoServiceProvider rsa = DecodePemPrivateKey(privateKey);
            var crypto = System.Security.Cryptography.MD5.Create();
            byte[] signData = rsa.SignData(bt, crypto);
            return Convert.ToBase64String(signData);
        }

        /// <summary>
        /// 验签
        /// </summary>
        /// <param name="content">待验签字符串</param>
        /// <param name="signedString">签名</param>
        /// <param name="publicKey">公钥</param>
        /// <param name="charset">编码格式</param>
        /// <returns>true(通过)，false(不通过)</returns>
        public static bool Verify(string content, string signedString, string publicKey, string charset = "utf-8")
        {
            bool result = false;
            byte[] bt = Encoding.GetEncoding(charset).GetBytes(content);
            byte[] signBytes = Convert.FromBase64String(signedString);
            RSAParameters paraPub = ConvertFromPemPublicKey(publicKey);
            RSACryptoServiceProvider rsaPub = new RSACryptoServiceProvider();
            rsaPub.ImportParameters(paraPub);
            //SHA1 crypto = new SHA1CryptoServiceProvider();
            var crypto = System.Security.Cryptography.MD5.Create();
            result = rsaPub.VerifyData(bt, crypto, signBytes);
            return result;
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="content">加密字符串</param>
        /// <param name="privateKey">私钥</param>
        /// <param name="charset">编码格式</param>
        /// <returns>明文</returns>
        public static string DecryptData(string content, string privateKey, string charset = "utf-8")
        {
            byte[] DataToDecrypt = Convert.FromBase64String(content);
            List<byte> result = new List<byte>();
            for (int j = 0; j < DataToDecrypt.Length / 128; j++)
            {
                byte[] buf = new byte[128];
                for (int i = 0; i < 128; i++)
                {
                    buf[i] = DataToDecrypt[i + 128 * j];
                }
                result.AddRange(decrypt(buf, privateKey));
            }

            byte[] source = result.ToArray();
            char[] asciiChars = new char[Encoding.GetEncoding(charset).GetCharCount(source, 0, source.Length)];
            Encoding.GetEncoding(charset).GetChars(source, 0, source.Length, asciiChars, 0);
            return new string(asciiChars);
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="content">加密文本，超长将分段处理</param>
        /// <param name="publicKey">公钥</param>
        /// <param name="charset"></param>
        /// <returns></returns>
        public static string EncryptData(string content, string publicKey, string charset = "utf-8")
        {
            byte[] bytes = Encoding.GetEncoding(charset).GetBytes(content);
            //RSAParameters paraPub = ConvertFromPublicKey(publicKey);
            //RSACryptoServiceProvider rsaPub = new RSACryptoServiceProvider();
            //rsaPub.ImportParameters(paraPub);
            //var data = rsaPub.Encrypt(bytes, false);
            //return Convert.ToBase64String(data);

            RSAParameters paraPub = ConvertFromPemPublicKey(publicKey);
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(paraPub);
                var bufferSize = (rsa.KeySize / 8 - 11);
                byte[] buffer = new byte[bufferSize];//待加密块

                using (MemoryStream msInput = new MemoryStream(bytes))
                {
                    using (MemoryStream msOutput = new MemoryStream())
                    {
                        int readLen;
                        while ((readLen = msInput.Read(buffer, 0, bufferSize)) > 0)
                        {
                            byte[] dataToEnc = new byte[readLen];
                            Array.Copy(buffer, 0, dataToEnc, 0, readLen);
                            byte[] encData = rsa.Encrypt(dataToEnc, false);
                            msOutput.Write(encData, 0, encData.Length);
                        }
                        byte[] result = msOutput.ToArray();
                        rsa.Clear();
                        return Convert.ToBase64String(result);
                    }
                }
            }
        }
        #region 内部方法 

        private static byte[] decrypt(byte[] data, string privateKey)
        {
            RSACryptoServiceProvider rsa = DecodePemPrivateKey(privateKey);
            return rsa.Decrypt(data, false);
        }

        private static RSACryptoServiceProvider DecodePemPrivateKey(string pemstr)
        {
            byte[] pkcs8privatekey;
            pkcs8privatekey = Convert.FromBase64String(pemstr);
            if (pkcs8privatekey != null)
            {
                RSACryptoServiceProvider rsa = DecodePrivateKeyInfo(pkcs8privatekey);
                return rsa;
            }
            else
                return null;
        }

        private static RSACryptoServiceProvider DecodePrivateKeyInfo(byte[] pkcs8)
        {
            byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] seq = new byte[15];

            MemoryStream mem = new MemoryStream(pkcs8);
            int lenstream = (int)mem.Length;
            BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading
            byte bt = 0;
            ushort twobytes = 0;

            try
            {
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)    //data read as little endian order (actual data order for Sequence is 30 81)
                    binr.ReadByte();    //advance 1 byte
                else if (twobytes == 0x8230)
                    binr.ReadInt16();    //advance 2 bytes
                else
                    return null;

                bt = binr.ReadByte();
                if (bt != 0x02)
                    return null;

                twobytes = binr.ReadUInt16();

                if (twobytes != 0x0001)
                    return null;

                seq = binr.ReadBytes(15);        //read the Sequence OID
                if (!CompareBytearrays(seq, SeqOID))    //make sure Sequence for OID is correct
                    return null;

                bt = binr.ReadByte();
                if (bt != 0x04)    //expect an Octet string 
                    return null;

                bt = binr.ReadByte();        //read next byte, or next 2 bytes is  0x81 or 0x82; otherwise bt is the byte count
                if (bt == 0x81)
                    binr.ReadByte();
                else
                      if (bt == 0x82)
                    binr.ReadUInt16();
                //------ at this stage, the remaining sequence should be the RSA private key

                byte[] rsaprivkey = binr.ReadBytes((int)(lenstream - mem.Position));
                RSACryptoServiceProvider rsacsp = DecodeRSAPrivateKey(rsaprivkey);
                return rsacsp;
            }

            catch (Exception)
            {
                return null;
            }

            finally { binr.Close(); }

        }

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

        private static RSACryptoServiceProvider DecodeRSAPrivateKey(byte[] privkey)
        {
            byte[] MODULUS, E, D, P, Q, DP, DQ, IQ;

            // ---------  Set up stream to decode the asn.1 encoded RSA private key  ------
            MemoryStream mem = new MemoryStream(privkey);
            BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading
            byte bt = 0;
            ushort twobytes = 0;
            int elems = 0;
            try
            {
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)    //data read as little endian order (actual data order for Sequence is 30 81)
                    binr.ReadByte();    //advance 1 byte
                else if (twobytes == 0x8230)
                    binr.ReadInt16();    //advance 2 bytes
                else
                    return null;

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102)    //version number
                    return null;
                bt = binr.ReadByte();
                if (bt != 0x00)
                    return null;

                //------  all private key components are Integer sequences ----
                elems = GetIntegerSize(binr);
                MODULUS = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                E = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                D = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                P = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                Q = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                DP = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                DQ = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                IQ = binr.ReadBytes(elems);

                // ------- create RSACryptoServiceProvider instance and initialize with public key -----
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                RSAParameters RSAparams = new RSAParameters();
                RSAparams.Modulus = MODULUS;
                RSAparams.Exponent = E;
                RSAparams.D = D;
                RSAparams.P = P;
                RSAparams.Q = Q;
                RSAparams.DP = DP;
                RSAparams.DQ = DQ;
                RSAparams.InverseQ = IQ;
                RSA.ImportParameters(RSAparams);
                return RSA;
            }
            catch (Exception)
            {
                return null;
            }
            finally { binr.Close(); }
        }

        private static int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            byte lowbyte = 0x00;
            byte highbyte = 0x00;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)        //expect integer
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
                count = binr.ReadByte();    // data size in next byte
            else if (bt == 0x82)
            {
                highbyte = binr.ReadByte();    // data size in next 2 bytes
                lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;        // we already have the data size
            }
            while (binr.ReadByte() == 0x00)
            {    //remove high order zeros in data
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);        //last ReadByte wasn't a removed zero, so back up a byte
            return count;
        }

        /// <summary>
        /// 补全密文
        /// </summary>
        /// <param name="strCiphertext">密文</param>
        /// <param name="keySize">秘钥长度</param>
        /// <returns>补全后的密文</returns>
        private static string CorrectionCiphertext(string strCiphertext, int keySize = 1024)
        {
            int ciphertextLength = keySize / 8;
            byte[] data = Convert.FromBase64String(strCiphertext);
            var newData = new List<byte>(data);
            while (newData.Count < ciphertextLength)
            {
                newData.Insert(0, 0x00);
            }
            return Convert.ToBase64String(newData.ToArray());
        }
        #endregion


        #region 解析.net 生成的Pem
        private static RSAParameters ConvertFromPemPublicKey(string pemFileConent)
        {
            byte[] keyData = Convert.FromBase64String(pemFileConent);
            if (keyData.Length < 162)
            {
                throw new ArgumentException("pem file content is incorrect.");
            }
            byte[] pemModulus = new byte[128];
            byte[] pemPublicExponent = new byte[3];
            Array.Copy(keyData, 29, pemModulus, 0, 128);
            Array.Copy(keyData, 159, pemPublicExponent, 0, 3);
            RSAParameters para = new RSAParameters();
            para.Modulus = pemModulus;
            para.Exponent = pemPublicExponent;
            return para;
        }

        private static RSAParameters ConvertFromPrivateKey(string pemFileConent)
        {
            byte[] keyData = Convert.FromBase64String(pemFileConent);
            if (keyData.Length < 609)
            {
                throw new ArgumentException("pem file content is incorrect.");
            }

            int index = 11;
            byte[] pemModulus = new byte[128];
            Array.Copy(keyData, index, pemModulus, 0, 128);

            index += 128;
            index += 2;//141
            byte[] pemPublicExponent = new byte[3];
            Array.Copy(keyData, index, pemPublicExponent, 0, 3);

            index += 3;
            index += 4;//148
            byte[] pemPrivateExponent = new byte[128];
            Array.Copy(keyData, index, pemPrivateExponent, 0, 128);

            index += 128;
            index += ((int)keyData[index + 1] == 64 ? 2 : 3);//279
            byte[] pemPrime1 = new byte[64];
            Array.Copy(keyData, index, pemPrime1, 0, 64);

            index += 64;
            index += ((int)keyData[index + 1] == 64 ? 2 : 3);//346
            byte[] pemPrime2 = new byte[64];
            Array.Copy(keyData, index, pemPrime2, 0, 64);

            index += 64;
            index += ((int)keyData[index + 1] == 64 ? 2 : 3);//412/413
            byte[] pemExponent1 = new byte[64];
            Array.Copy(keyData, index, pemExponent1, 0, 64);

            index += 64;
            index += ((int)keyData[index + 1] == 64 ? 2 : 3);//479/480
            byte[] pemExponent2 = new byte[64];
            Array.Copy(keyData, index, pemExponent2, 0, 64);

            index += 64;
            index += ((int)keyData[index + 1] == 64 ? 2 : 3);//545/546
            byte[] pemCoefficient = new byte[64];
            Array.Copy(keyData, index, pemCoefficient, 0, 64);

            RSAParameters para = new RSAParameters();
            para.Modulus = pemModulus;
            para.Exponent = pemPublicExponent;
            para.D = pemPrivateExponent;
            para.P = pemPrime1;
            para.Q = pemPrime2;
            para.DP = pemExponent1;
            para.DQ = pemExponent2;
            para.InverseQ = pemCoefficient;
            return para;
        }

        #endregion
    }
}
