using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp.Net.Util.Aliyun.Oss
{
    public class FileUploadAliyunModel
    {
        public string AccessId { get; set; }
        public string AccessKey { get; set; }
        public string EndPoint { get; set; }
        public string BucketName { get; set; }
        public string ExpiratioDate { get; set; }
        public string Environment { get; set; }
    }
}
