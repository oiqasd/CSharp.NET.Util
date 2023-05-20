using Aliyun.OSS;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp.Net.Util.Aliyun.Oss
{
    /// <summary>
    /// OssManager对象
    /// https://github.com/aliyun/aliyun-oss-csharp-sdk
    /// </summary>
    public class OssManager
    {
        private static OssClient ossClient;

        public static OssClient GetInstance(string accessId, string accessKey, string endPoint)
        {
            if (ossClient == null)
            {
                ossClient = new OssClient(endPoint, accessId, accessKey);
            }
            return ossClient;
        }
    }
}
