namespace CSharp.Net.Util
{
    internal class LoggerStopedException : AppException
    {
        public LoggerStopedException() : base(AExceptionCode.LoggerStoped, "Logger stop.") { }
    }
}
