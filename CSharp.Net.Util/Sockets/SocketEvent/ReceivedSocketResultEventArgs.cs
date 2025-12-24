using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 收到返回值包事件参数
    /// </summary>
    /// </summary>
    public class ReceivedSocketResultEventArgs : EventArgs
    {
        private SocketResult _SocketResult;
        /// <summary>
        /// 数据
        /// </summary>
        public SocketResult SocketResult
        {
            get { return _SocketResult; }
            set { _SocketResult = value; }
        }

        public ReceivedSocketResultEventArgs(SocketResult socketResult)
        {
            _SocketResult = socketResult;
        }
    }

    /// <summary>
    /// 客户端离线事件参数
    /// </summary>
    public class SocketClientOfflineEventArgs : EventArgs
    {
        private string _SocketClientId;
        /// <summary>
        /// Socket客户端ID
        /// </summary>
        public string SocketClientId
        {
            get { return _SocketClientId; }
            set { _SocketClientId = value; }
        }

        public SocketClientOfflineEventArgs(string socketClientId)
        {
            _SocketClientId = socketClientId;
        }
    }

    /// <summary>
    /// Socket客户端注册事件参数
    /// </summary>
    public class SocketClientRegisterEventArgs : EventArgs
    {
        private string _SocketClientId;
        /// <summary>
        /// Socket客户端ID
        /// </summary>
        public string SocketClientId
        {
            get { return _SocketClientId; }
            set { _SocketClientId = value; }
        }

        /// <summary>
        /// Socket客户端注册事件参数
        /// </summary>
        public SocketClientRegisterEventArgs(string socketClientId)
        {
            _SocketClientId = socketClientId;
        }
    }

    /// <summary>
    /// Socket事件参数
    /// </summary>
    public class SocketReceivedEventArgs : EventArgs
    {
        private MsgContent _Content;
        /// <summary>
        /// 消息数据
        /// </summary>
        public MsgContent Content
        {
            get { return _Content; }
            set { _Content = value; }
        }

        private CallbackSocket _Callback;
        /// <summary>
        /// 回调类
        /// </summary>
        public CallbackSocket Callback
        {
            get { return _Callback; }
            set { _Callback = value; }
        }

        public SocketReceivedEventArgs(MsgContent data)
        {
            _Content = data;
        }
    }

    /// <summary>
    /// 操作结果事件参数
    /// </summary>
    public class SocketResultEventArgs : EventArgs
    {
        private SocketResult _SocketResult;
        /// <summary>
        /// 数据
        /// </summary>
        public SocketResult SocketResult
        {
            get { return _SocketResult; }
            set { _SocketResult = value; }
        }

        private ClientSocket _ClientSocket;
        /// <summary>
        /// 客户端Socket对象
        /// </summary>
        public ClientSocket ClientSocket
        {
            get { return _ClientSocket; }
            set { _ClientSocket = value; }
        }

        public SocketResultEventArgs(ClientSocket clientSocket, SocketResult socketResult)
        {
            _ClientSocket = clientSocket;
            _SocketResult = socketResult;
        }
    }
}
