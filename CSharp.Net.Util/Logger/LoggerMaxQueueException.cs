namespace CSharp.Net.Util
{
    internal class LoggerMaxQueueException : System.InvalidOperationException
    {
        public LoggerMaxQueueException() : base("Logger queue has reached its maximum length.") { }
        public LoggerMaxQueueException(string message) : base(message) { }
    }
}
