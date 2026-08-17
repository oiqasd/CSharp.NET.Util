namespace CSharp.Net.Util
{
    internal class LoggerStopedException : System.InvalidOperationException
    {
        public LoggerStopedException() : base("Logger has stopped.") { }
    }
}
