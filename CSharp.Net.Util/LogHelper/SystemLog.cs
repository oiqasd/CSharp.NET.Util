using CSharp.Net.Util.NewtJson;
using System;

namespace CSharp.Net.Util
{
    class SystemLog : ISystemLog
    {
        public SystemLog()
        {
            CreateTime = DateTime.Now;
            Level = LogLevel.Info;
        }
        public string TransferId { get; set; }
        public string AppId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public DateTime CreateTime { get; set; }
        public long ElapsedTime { get; set; }
        public LogLevel Level { get; set; }

        public override string ToString()
        {
            return JsonHelper.Serialize(this);
        }
    }
}
