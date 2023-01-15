using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace CSharp.Net.Standard.Util.Compression
{
    /// <summary>
    /// GZip压缩帮助类
    /// </summary>
    public static class GZipHelper
    {
        /// <summary>
        /// 压缩
        /// </summary>
        /// <param name="bytes">输入的byte数组</param>
        /// <returns>压缩后的byte数组</returns>
        public static byte[] Compress(byte[] bytes)
        {
            MemoryStream ms = new MemoryStream();
            GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true);
            zip.Write(bytes, 0, bytes.Length);
            zip.Close();
            byte[] buffer = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(buffer, 0, buffer.Length);
            ms.Close();
            return buffer;
        }

        /// <summary>
        /// 解压缩
        /// </summary>
        /// <param name="bytes">压缩后的byte数组</param>
        /// <returns>解压缩后的byte数组</returns>
        public static byte[] Decompress(byte[] bytes)
        {
            MemoryStream ms = new MemoryStream(bytes);
            GZipStream zip = new GZipStream(ms, CompressionMode.Decompress, true);
            MemoryStream msreader = new MemoryStream();
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

        /// <summary>
        /// 对字符串进行压缩
        /// </summary>
        /// <param name="str">待压缩的字符串</param>
        /// <returns>压缩后的字符串</returns>
        public static string CompressString(string str)
        {
            var compressString = string.Empty;
            byte[] compressBeforeByte = Encoding.UTF8.GetBytes(str);
            byte[] compressAfterByte = Compress(compressBeforeByte);

            compressString = Convert.ToBase64String(compressAfterByte);
            return CompressStringReplace(compressString);
        }

        /// <summary>
        /// 对字符串进行解压缩
        /// </summary>
        /// <param name="str">待解压缩的字符串</param>
        /// <returns>解压缩后的字符串</returns>
        public static string DecompressString(string str)
        {
            var decompressString = string.Empty;
            str = DecompressStringReplace(str);
            byte[] compressBeforeByte = Convert.FromBase64String(str);
            byte[] compressAfterByte = Decompress(compressBeforeByte);
            decompressString = Encoding.UTF8.GetString(compressAfterByte);
            return decompressString;
        }

        private static string CompressStringReplace(string compressString)
        {
            return compressString.Replace('+', '*').Replace('/', '^').Replace('=', '.').Replace(' ', '~');
        }

        private static string DecompressStringReplace(string decompressString)
        {
            return decompressString.Replace('*', '+').Replace('^', '/').Replace('.', '=').Replace('~', ' ');
        }
    }
}
