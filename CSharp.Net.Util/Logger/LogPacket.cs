namespace CSharp.Net.Util.Logger
{
    internal sealed class LogPacket
    {
        public LogLevel Level;
        public byte[] Buffer = default!;
        public int Length;
    }
}
