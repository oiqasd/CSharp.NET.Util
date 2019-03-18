using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// 数字转换大写汉字
/// </summary>
public class TraditionalNumber
{
    #region 数字转换大写汉字
    private static string[] NumChineseCharacter = new string[] { "零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖" };
    /// <summary>
    /// 数字转换成大写汉字主函数
    /// </summary>
    /// <returns>返回转换后的大写汉字</returns>
    public static string NumberString(string m)
    {
        string bb = string.Empty;
        string xs = "0";
        string Num = "";

        if (m.Contains("."))
        {
            string[] ss = m.Split('.');
            xs = ss[1];
            Num = ss[0];
        }
        else
        {
            Num = m;
        }

        if (Num.Length <= 4)
        {
            bb = Convert4(Num);
        }
        else if (Num.Length > 4 && Num.Length <= 8)
        {
            bb = Convert4(Num.Substring(0, Num.Length - 4)) + "万";
            bb += Convert4(Num.Substring(Num.Length - 4, 4));
        }
        else if (Num.Length > 8 && Num.Length <= 12)
        {
            bb = Convert4(Num.Substring(0, Num.Length - 8)) + "亿";
            if (Convert4(Num.Substring(Num.Length - 8, 4)) == "")
                if (Convert4(Num.Substring(Num.Length - 4, 4)) != "")
                    bb += "零";
                else
                    bb += "";
            else
                bb += Convert4(Num.Substring(Num.Length - 8, 4)) + "万";
            bb += Convert4(Num.Substring(Num.Length - 4, 4));
        }
        //if (Num.Length == 2 && Num.StartsWith("1"))
        //{
        //    bb = bb.Substring(1);
        //}
        bb = bb + "元整";
        if (int.Parse(xs) > 0)
        {
            bb = bb.Substring(0, bb.Length - 1);
            for (int xj = 0; xj < xs.Length; xj++)
            {
                bb = bb + Convert1(xs.Substring(xj, 1));
                if (xj == 0)
                {
                    bb = bb + "角";
                }
                else if (xj == 1)
                {
                    bb = bb + "分";
                }
            }
        }

        return bb;
    }
    private static string Convert4(string Num)
    {
        string bb = "";
        if (Num.Length == 1)
        {
            bb = ConvertString(Num);
        }
        else if (Num.Length == 2)
        {
            bb = ConvertString(Num);
            bb = Convert2(bb);
        }
        else if (Num.Length == 3)
        {
            bb = ConvertString(Num);
            bb = Convert3(bb);
        }
        else
        {
            bb = ConvertString(Num);
            string cc = "";
            string len = bb.Substring(0, 4);
            if (len != "零零零零")
            {
                len = bb.Substring(0, 3);
                if (len != "零零零")
                {
                    bb = bb.Replace("零零零", "");
                    if (bb.Length == 1)
                    {
                        bb = bb.Substring(0, 1) + "仟";
                    }
                    else
                    {
                        if (bb.Substring(0, 1) != "零" && bb.Substring(0, 2) != "零")
                            cc = bb.Substring(0, 1) + "仟";
                        else
                            cc = bb.Substring(0, 1);
                        bb = cc + Convert3(bb.Substring(1, 3));
                    }
                }
                else
                {
                    bb = bb.Replace("零零零", "零");
                }
            }
            else
            {
                bb = bb.Replace("零零零零", "");
            }
        }
        return bb;
    }
    private static string Convert3(string Num)
    {
        string bb = ""; string cc = "";
        string len = Num.Substring(0, 2);
        if (len != "零零")
        {
            bb = Num.Replace("零零", "");
            if (bb.Length == 1)
            {
                bb = bb.Substring(0, 1) + "佰";
            }
            else
            {
                if (bb.Substring(0, 1) != "零")
                    cc = bb.Substring(0, 1) + "佰";
                else
                    cc = bb.Substring(0, 1);
                bb = cc + Convert2(bb.Substring(1, 2));
            }
        }
        else
        {
            bb = Num.Replace("零零", "零");
        }
        return bb;
    }
    private static string Convert2(string Num)
    {
        string bb = ""; string cc = "";
        string len = Num.Substring(0, 1);
        if (len != "零")
        {
            bb = Num.Replace("零", "");
            if (bb.Length == 1)
            {
                cc = bb.Substring(0, 1) + "拾";
            }
            else
            {
                cc = bb.Substring(0, 1) + "拾";
                cc += bb.Substring(1, 1);
            }
        }
        else
            cc = Num;
        return cc;
    }
    private static string ConvertString(string Num)
    {
        string bb = "";
        for (int i = 0; i < Num.Length; i++)
        {
            bb += NumChineseCharacter[int.Parse(Num.Substring(i, 1))];
        }
        return bb;
    }
    private static string Convert1(string Num)
    {
        return NumChineseCharacter[int.Parse(Num.Substring(0, 1))];
    }
    #endregion

}