namespace CSharp.Net.Util.Compression
{
    /// <summary>
    /// http://www.cnblogs.com/luoxiaojie/articles/1818885.html
    /// </summary>
    public class Compressor
    {
        private static ICompressor _gzipCompressor = new GZipCompressor();
        private static ICompressor _deflateCompressor = new DeflateCompressor();
        private static ICompressor _soaGZipCompressor = new SOAGZipCompressor();

        public static ICompressor GZip 
        { 
            get { return _gzipCompressor; } 
        }

        public static ICompressor SOAGZip
        {
            get { return _soaGZipCompressor; }
        }

        public static ICompressor Deflate 
        { 
            get { return _deflateCompressor; } 
        }
    }
}