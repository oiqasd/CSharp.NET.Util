using CSharp.Net.Util;
using CSharp.Net.Util.Cryptography;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;


namespace UnitTestProject1
{

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {

        }

        [TestMethod]
        public void TestRSAUtil()
        {





            string pub = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQC7PyjMEuniN6BPn8oqzIZ6AO1NjSTO9R3adCCIwKfKIEoWXXM+tHDpktdPKSaAsWJPTNAGvEvtxOfzXib/EMXKqD0eUy5MatfpRjRdf1hJVimmfrb09Qx2j7CsKLy7nD23m4xubdYBwvkjMwt/L3JxB5D6qryW1wei/j1c+/OCxQIDAQAB";
            string pri = "MIICXQIBAAKBgQC7PyjMEuniN6BPn8oqzIZ6AO1NjSTO9R3adCCIwKfKIEoWXXM+tHDpktdPKSaAsWJPTNAGvEvtxOfzXib/EMXKqD0eUy5MatfpRjRdf1hJVimmfrb09Qx2j7CsKLy7nD23m4xubdYBwvkjMwt/L3JxB5D6qryW1wei/j1c+/OCxQIDAQABAoGAT7vGYJgRNf4f6qgNS4pKHTu10RcwPFyOOM7IZ9M5380+HyXuBB6MEjowKwpH1fcy+LepwaR+5KG7b5uBGY4H2ticMtdysBd9gLwnY4Eh4j7LCWE54HvELpeWXkWpFQdb/NQhcqMAGwYsTnRPdBqkrUmJBTYqEGkIlqCQ5vUJOCECQQDhe0KGmbq1RWp6TDvgpA2dUmlt2fdP8oNW8O7MvbDaQRduoZnVRTPYCDKfzFqpNXL1hAYgth1N0vzDnv3VoLcpAkEA1JcY+rLv5js1g5Luv8LaI5/3uOg0CW7fmh/LfGuz8k/OxASN+cAOUjPHrxtc5xn1zat4/bnV5GEdlOp/DhquPQJBAIV2Fsdi4M+AueiPjPWHRQO0jvDVjfwFOFZSn5YSRUa6NmtmPY6tumUJXSWWqKb1GwlVTuc3xBqXYsNLLUWwLhkCQQDJUJCiD0LohhdGEqUuSKnj5H9kxddJO4pZXFSI7UEJbJQDwcBkyn+FTm2BH+tZGZdQfVnlA89OJr0poOpSg+eNAkAKY85SR9KASaTiDBoPpJ8N805XEhd0Kq+ghzSThxL3fVtKUQLiCh7Yd8oMd/G5S3xWJHUXSioATT8uPRH2bOb/";

            string content = "BirthDay=1990-09-10&BirthPlace=安徽&CName=秦骏&CNameSpelling=qinjun&ContactInformation=15555230910&CountryId=1&CredentialsNum=321312313&CredentialsType=1&Email=jun.qin@xiaobao100.com&EName=qinjun&EntranceYear=2009&FamilyInfos=System.Collections.Generic.List`1[Xiaobao.Cloud.OpenApi.CSharp.Model.CreateStudentFamilyRequest]&GraduateSchoolName=集美&IsGetAccommodation=true&NationId=1&Remark=212121&SchoolId=108&Sex=1&StudySection=1&StuNumber=2312313";
            if (1 == 2)
            {
                var keys = new CrypRSA().CreateKey();
                pri = keys.Item1;
                pub = keys.Item2;
            }

            var signdata = CrypRSA.Sign(content, pri);

            var cksign = CrypRSA.CheckSign(content, signdata, pub);

            string encal = CrypRSA.Encrypt(content, pub);
            string deval = CrypRSA.Decrypt(encal, pri);

            Assert.IsTrue(deval == content);

        }

        [TestMethod]
        public void TestIO()
        {
            System.IO.FileStream fs = new System.IO.FileStream("D:\\BinaryStreamTest.txt", FileMode.OpenOrCreate);
            BinaryReader re = new BinaryReader(fs);
            BinaryWriter w = new BinaryWriter(fs);
            //以二进制方式向创建的文件中写入内容   
            w.Write(666);                   //  整型  
            w.Write(66.6f);                // 浮点型  
            w.Write(6.66);                // double型  
            w.Write(true);                 // 布尔型  
            w.Write("六六六");         // 字符串型  

            w.Close();
            fs.Close();
        }

        [TestMethod]
        public void TestTransNumber()
        {
            decimal m = 858521685.25M;

            var s = TraditionalNumber.NumberString(m);

            Console.WriteLine(s);
        }
    }
}
