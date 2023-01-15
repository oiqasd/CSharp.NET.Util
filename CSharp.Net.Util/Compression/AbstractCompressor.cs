using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace CSharp.Net.Standard.Util.Compression
{
    public abstract class AbstractCompressor : ICompressor
    {
        public string CompressBytesToString(byte[] bytesToCompressed)
        {
            byte[] bytesCompressed = Compress(bytesToCompressed);
            return Convert.ToBase64String(bytesCompressed);
        }

        public byte[] DecompressStringToBytes(string str)
        {
            byte[] bytesToDecompressed = Convert.FromBase64String(str);
            byte[] bytesDecompressed = Decompress(bytesToDecompressed);
            return bytesDecompressed;
        }

        /// <summary>
        /// 对字符串进行压缩
        /// </summary>
        /// <param name="str">待压缩的字符串</param>
        /// <returns>压缩后的字符串</returns>
        public string CompressStringToString(string str, Encoding encoding)
        {
            var fixedEncoding = FixDefaultEncoding(encoding);
            byte[] bytesToCompressed = fixedEncoding.GetBytes(str);
            byte[] bytesCompressed = Compress(bytesToCompressed);
            return Convert.ToBase64String(bytesCompressed);
        }

        public string CompressStringToString(string str)
        {
            return CompressStringToString(str, Encoding.UTF8);
        }

        /// <summary>
        /// 对字符串进行解压缩
        /// </summary>
        /// <param name="str">待解压缩的字符串</param>
        /// <returns>解压缩后的字符串</returns>
        public string DecompressStringToString(string str, Encoding encoding)
        {
            var fixedEncoding = FixDefaultEncoding(encoding);
            byte[] bytesToDecompressed = Convert.FromBase64String(str);
            byte[] bytesDecompressed = Decompress(bytesToDecompressed);
            return fixedEncoding.GetString(bytesDecompressed);
        }

        public string DecompressStringToString(string str)
        {
            return DecompressStringToString(str, Encoding.UTF8);
        }

        // <summary>
        /// 对byte数组进行压缩
        /// </summary>
        /// <param name="data">待压缩的byte数组</param>
        /// <returns>压缩后的byte数组</returns>
        public virtual byte[] Compress(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (Stream zip = CreateCompressStream(ms, CompressionMode.Compress))
                {
                    zip.Write(data, 0, data.Length);
                    zip.Close();

                    byte[] buffer = new byte[ms.Length];
                    ms.Position = 0;
                    ms.Read(buffer, 0, buffer.Length);
                    ms.Close();
                    return buffer;
                }
            }
        }

        /// <summary>
        /// 对byte数组进行解压
        /// </summary>
        /// <param name="data">压缩的byte数组</param>
        /// <returns>解压后的byte数组</returns>
        public virtual byte[] Decompress(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (Stream zip = CreateCompressStream(ms, CompressionMode.Decompress))
                {
                    using (MemoryStream msreader = new MemoryStream())
                    {
                        byte[] buffer = new byte[0x1000];
                        while (true)
                        {
                            int reader = zip.Read(buffer, 0, buffer.Length);
                            if (reader <= 0)
                            {
                                break;
                            }
                            msreader.Write(buffer, 0, reader);
                        }
                        zip.Close();
                        ms.Close();
                        msreader.Position = 0;
                        buffer = msreader.ToArray();
                        msreader.Close();
                        return buffer;
                    }
                }
            }
        }

        protected abstract Stream CreateCompressStream(Stream stream, CompressionMode mode);

        private Encoding FixDefaultEncoding(Encoding encoding)
        {
            Encoding fixedEncoding = encoding;
            if (encoding == null)
            {
                fixedEncoding = Encoding.UTF8;
            }
            return fixedEncoding;
        }
    }
}