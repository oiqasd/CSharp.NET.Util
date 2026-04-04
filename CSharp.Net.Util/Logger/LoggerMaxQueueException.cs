namespace CSharp.Net.Util
{
    internal class LoggerMaxQueueException:AppException
    {
        public LoggerMaxQueueException() : base(AExceptionCode.LoggerMaxQueue, "LoggerMaxQueue") { }
        public LoggerMaxQueueException(string message) : base(AExceptionCode.LoggerMaxQueue, message) { }
    }
}
