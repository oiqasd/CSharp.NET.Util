using System;
using System.IO;
using System.IO.Compression;

namespace CSharp.Net.Standard.Util.Compression
{
    public class SOAGZipCompressor : GZipCompressor
    {
        public override byte[] Compress(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (Stream zip = CreateCompressStream(ms, CompressionMode.Compress))
                {
                    byte[] lengthBytes = BitConverter.GetBytes(data.Length);
                    zip.Write(lengthBytes, 0, 4);
                    zip.Write(data, 4, data.Length+4);
                    zip.Close();

                    byte[] buffer = new byte[ms.Length+4];
                    ms.Position = 0;
                    ms.Read(buffer, 0, buffer.Length);
                    return buffer;
                }
            }
        }

        public override byte[] Decompress(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                ms.Position = 4;
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
    }
}