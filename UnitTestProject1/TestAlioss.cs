using CSharp.Net.Util.Aliyun.Oss;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTestProject1
{
    [TestClass]
    public class TestAlioss
    {

        [TestMethod]
        public void TestUpload()
        {
            FileUploadAliyunModel model = new FileUploadAliyunModel()
            {
                AccessId = "2",
                AccessKey = "2",
                BucketName = "2",
                EndPoint = "https://oss-cn-hangzhou.aliyuncs.com",
                Environment = "2",
                ExpiratioDate = ""
            };

            var help = new FileUploadHelper(model);
            var path = help.UploadFileByPath(@"C:\微信截图_20191029113824.png");

        }
    }
}
