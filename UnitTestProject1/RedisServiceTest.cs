using Longjubank.FrameWorkCore.RedisManger;
using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace XUnitTestProject.Framework
{
    public class RedisServiceTest

    {
        private RedisService _RedisServer;
        private RedisService RedisServer
        {
            get
            {
                if (_RedisServer == null)
                {
                    RedisServiceConfig config = new RedisServiceConfig()
                    {
                        Address = "192.168.8.150:6339",
                        PoolSize = 20,
                        DefaultDb = 1,
                        Pwd = ""
                    };
                    _RedisServer = new RedisService(config);
                }
                return _RedisServer;
            }
        }
        class TestObj
        {
            public string field1 { get; set; }
            public int field2 { get; set; }
            public DateTime field3 { get; set; }
        }

        int maxtimes = 1000;
        [Fact(DisplayName = "���Դ�ȡЧ��")]
        public void TestSpeed()
        {
            ThreadPool.SetMaxThreads(5, 5);


            int index = 0;
            while (index < maxtimes)
            {
                TestDoHash();
                //SpeedTest(index);
                //ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(SpeedTest), index);
                //var result = RedisServer.StringSetAsync("first", " test1", new TimeSpan(0, 0, 1));
                //var value = RedisServer.StringGetAsync("first");
                //Xunit.Assert.True(value.Result == " test1", "�����ȡ����");
                index++;
            }
        }
        void SpeedTest(object obj)
        {
            TestDoString();
        }
        [Fact(DisplayName = "StringSet����string�Ĵ�ȡ")]
        public void TestDoString()
        {
            bool result1 = RedisServer.StringSet("first", " test1", new TimeSpan(0, 0, 1));
            bool result2 = RedisServer.StringSet("second", "test2", new TimeSpan(0, 0, 2));
            var obj = new TestObj
            {
                field1 = "abcde",
                field2 = 12345,
                field3 = DateTime.Now
            };
            bool result3 = RedisServer.StringSet("third", obj, new TimeSpan(0, 0, 3));
            bool result4 = RedisServer.StringSet("forth", 10000, new TimeSpan(0, 0, 4));
            //Xunit.Assert.True(result1 & result2 & result3 & result4, "������������");

            TestObj value = RedisServer.StringGet<TestObj>("third");

            //Xunit.Assert.True(obj.field2 == value.field2, "�����ȡ����");

            var list = new List<string>(new string[] { "first", "second", "third", "forth" });


            var RedisValues = RedisServer.StringGet(list);
            //Xunit.Assert.Contains(" test1", RedisValues);
            //Console.WriteLine(JsonConvert.SerializeObject(RedisValues));

            //Thread.Sleep(2000);
            //var second = RedisServer.StringGet("second");
            //Xunit.Assert.True(string.IsNullOrWhiteSpace(second), "������������");

            int forth = RedisServer.StringGet<int>("forth");
            //Xunit.Assert.True(forth == 10000, "δ������������");


        }
        [Fact(DisplayName = "HashSet����Hash�Ĵ�ȡ")]
        public void TestDoHash()
        {
            bool result1 = RedisServer.HashSetAsync("100201_fort", " test11", "1").Result;
            bool result2 = RedisServer.HashSetAsync("100201_fort", " test21", "2").Result;
            bool result3 = RedisServer.HashSetAsync("100201_fort", " test31", "3").Result;
            bool result4 = RedisServer.HashSetAsync("100201_fort", " test41", "4").Result;

            bool exits = RedisServer.HashExistsAsync("100201_fort", " test11").Result;
            Xunit.Assert.True(exits, "�����ж�����");
            bool notexits = RedisServer.HashExistsAsync("100201_fort", " testxxxxxx").Result;
            Xunit.Assert.True(!notexits, "�������ж�����");
            result3 = RedisServer.HashSetAsync("100201_fort", " test3", "3333333333333333333").Result;

            Dictionary<string, string> dic = RedisServer.HashGetAllAsync<string>("100201_for111t").Result;
            if (RedisServer.HashExistsAsync("100201_fort", "test3").Result)
            {
                var t3 = RedisServer.HashGetAsync<string>("100201_fort", "test3").Result;
            }
            Xunit.Assert.True(dic[" test3"] == "3333333333333333333", "��ȡֵ��ȷ");

            var deleteresult = RedisServer.HashDeleteAsync("100201_fort", " test41").Result;
            Xunit.Assert.True(deleteresult, "HashSet����ɾ������");

        }
    }
}
