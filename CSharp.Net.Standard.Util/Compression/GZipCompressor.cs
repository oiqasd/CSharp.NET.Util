using System.IO;
using System.IO.Compression;

namespace CSharp.Net.Standard.Util.Compression
{
    public class GZipCompressor : AbstractCompressor
    {
        protected override Stream CreateCompressStream(Stream stream, CompressionMode mode)
        {
            return new GZipStream(stream, mode, true);
        }
    }
}
