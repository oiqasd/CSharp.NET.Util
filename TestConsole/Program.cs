using CSharp.Net.Standard.Util;
using System;
using System.ComponentModel;
using CSharp.Net.Standard.Util.Extensions;
using CSharp.Net.Standard.Util.Helper;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {

            for (var i = 0; i < 999; i++)
            {

                Console.WriteLine(Utils.GetRandom(9));
            }
            //   var s = Utils.GetRandom();


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
