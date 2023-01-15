using CSharp.Net.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;

namespace TestConsole
{
    public class stu
    {
        public string name { get; set; }
        public string key { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            List<stu> ls = new List<stu>() { new stu { key = "12", name = "amg" }, new stu { key = "13", name = "tmd" } };
            var pa = new PageArgument() { PageIndex = 1, PageSize = 12 };
            PageList<stu> ts = (from a in ls select a).AsQueryable().ToPageList(pa);

            var re = JsonHelper.Serialize(ts);

            string file = "D:\\log.txt";

            for (var i = 0; i < 2; i++)
            {
                using (System.IO.StreamWriter sw = new StreamWriter(file, false))
                {
                    sw.BaseStream.Seek(0, System.IO.SeekOrigin.End);
                    sw.WriteLine(Utils.GetRandom(9));
                }

            }


            for (var i = 0; i < 999; i++)
            {

                Console.WriteLine(Utils.GetRandom(9));
            }
            //   var s = Utils.GetRandom();

            DataTable tl = new DataTable();

            tl.GetRow(1).GetString("aa");
            Console.ReadLine();

        }
    }

    enum usertype
    {

        [Description("中国"), EDescription("enchina")]
        zhongguo = 10,
        [Description("中国20"), EDescription("enchina20")]
        riebn = 20,
        [Description("中国30"), EDescription("enchina30")]
        meiguo = 30

    }
}
