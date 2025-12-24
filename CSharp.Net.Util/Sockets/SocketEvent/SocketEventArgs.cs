using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp.Net.Util
{
    public class SocketEventArgs : EventArgs
    {
        public byte[] ReceivedData { get; set; }
        public byte[] ResponseData { get; set; }
        public string Message { get; set; }
        public string Response { get; set; }
        public string Address { get; set; }
    }
}
