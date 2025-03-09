using System.Net.Sockets;

namespace CSharp.Net.Util
{
    internal class SocketPacketException: SocketException
    {
        string message ;
        public SocketPacketException(string message)
        {
            this.message = message;
        }
        public override string Message => message;
    }
}
