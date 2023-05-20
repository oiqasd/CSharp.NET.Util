using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp.Net.Util.Aliyun.Oss
{
    public class FileUploadAliyunModel
    {
        public string AccessId { get; set; }
        public string AccessKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string EndPoint { get; set; }
        /// <summary>
        /// 桶
        /// </summary>
        public string BucketName { get; set; }
        /// <summary>
        /// 子目录 可空
        /// 格式：aaa/bbb/
        /// </summary>
        public string Path { get; set; }
        public string ExpiratioDate { get; set; }
        public string Environment { get; set; }
    }
}
