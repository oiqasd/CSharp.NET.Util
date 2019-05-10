using CSharp.Net.Standard.Util;
using System;
using System.ComponentModel;
using System.Data;
using System.IO;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
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
