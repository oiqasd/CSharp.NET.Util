using System.Text;

namespace CSharp.Net.Util.Compression
{
    public interface ICompressor
    {
        string CompressBytesToString(byte[] bytesToCompressed);

        byte[] DecompressStringToBytes(string str);

        /// <summary>
        /// 对字符串进行压缩
        /// </summary>
        /// <param name="str">待压缩的字符串</param>
        /// <returns>压缩后的字符串</returns>
        string CompressStringToString(string str, Encoding encoding);

        /// <summary>
        /// 对字符串进行解压缩
        /// </summary>
        /// <param name="str">待解压缩的字符串</param>
        /// <returns>解压缩后的字符串</returns>
        string CompressStringToString(string str);

        /// <summary>
        /// 对字符串进行解压缩
        /// </summary>
        /// <param name="str">待解压缩的字符串</param>
        /// <returns>解压缩后的字符串</returns>
        string DecompressStringToString(string str, Encoding encoding);

        /// <summary>
        /// 对字符串进行解压缩
        /// </summary>
        /// <param name="str">待解压缩的字符串</param>
        /// <returns>解压缩后的字符串</returns>
        string DecompressStringToString(string str);

        /// <summary>
        /// 对byte数组进行压缩
        /// </summary>
        /// <param name="data">待压缩的byte数组</param>
        /// <returns>压缩后的byte数组</returns>
        byte[] Compress(byte[] data);

        /// <summary>
        /// 对byte数组进行解压
        /// </summary>
        /// <param name="data">压缩的byte数组</param>
        /// <returns>解压后的byte数组</returns>
        byte[] Decompress(byte[] data);
    }
}