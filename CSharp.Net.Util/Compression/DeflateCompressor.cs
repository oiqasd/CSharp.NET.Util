using System.IO;
using System.IO.Compression;

namespace CSharp.Net.Standard.Util.Compression
{
    public class DeflateCompressor : AbstractCompressor
    {
        protected override Stream CreateCompressStream(Stream stream, CompressionMode mode)
        {
            return new DeflateStream(stream, mode, true);
        }
    }
}
