using iTextSharp.text;
using iTextSharp.text.pdf;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Image = iTextSharp.text.Image;

namespace CSharp.Net.Util.Pdf;

public class PdfHelper
{
    /// <summary>
    /// iTextSharp 获取文件byte
    /// </summary>
    /// <param name="fontPath">字体路径</param>
    /// <param name="templatePath">模版路径</param>
    /// <param name="printPara">填充表单数据</param>
    /// <returns></returns>
    public static byte[] GetPdfByte(string fontPath, string templatePath, Dictionary<string, string> printPara)
    {
        //获取中文字体，第三个参数表示为是否潜入字体，但只要是编码字体就都会嵌入。
        BaseFont baseFont = BaseFont.CreateFont($"{fontPath}", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
        //读取模板文件
        PdfReader reader = new PdfReader(templatePath);
        //创建文件流用来保存填充模板后的文件
        MemoryStream stream = new MemoryStream();
        PdfStamper stamp = new PdfStamper(reader, stream);
        //设置表单字体，在高版本有用，高版本加入这句话就不会插入字体，低版本无用
        stamp.AcroFields.AddSubstitutionFont(baseFont);
        AcroFields form = stamp.AcroFields;
        //表单文本框是否锁定
        stamp.FormFlattening = true;

        //填充表单,para为表单的一个（属性-值）字典
        foreach (KeyValuePair<string, string> parameter in printPara)
        {
            //要输入中文就要设置域的字体;
            form.SetFieldProperty(parameter.Key, "textfont", baseFont, null);
            //为需要赋值的域设置值;
            form.SetField(parameter.Key, parameter.Value);
        }
        //按顺序关闭io流
        stamp.Close();
        reader.Close();

        return stream.ToArray();
    }

    public static void AddWater(string pdfPath, string words)
    {
        PdfReader pdfReader = null;
        PdfStamper pdfStamper = null;
        try
        {
            pdfReader = new PdfReader(pdfPath);

            int numberOfPages = pdfReader.NumberOfPages;
            iTextSharp.text.Rectangle psize = pdfReader.GetPageSize(1);

            float width = psize.Width;
            float height = psize.Height;
            MemoryStream stream = new MemoryStream();
            //pdfStamper = new PdfStamper(pdfReader, stream);
            pdfStamper = new PdfStamper(pdfReader, new FileStream(pdfPath + ".pdf", FileMode.Create));

            PdfContentByte waterMarkContent;
            PdfGState gs = new PdfGState();
            gs.FillOpacity = 0.2f;

            //iTextSharp.text.Image imgTemp = iTextSharp.text.Image.GetInstance(ModelPicName);
            //float imgWidth = (float)Math.Cos(Math.PI / 4) * imgTemp.Width + 100;
            //float imgHeight = (float)Math.Sin(Math.PI / 4) * imgTemp.Width + 100;
            iTextSharp.text.Image image1 = CreateWater(words);

            //每一页加水印,也可以设置某一页加水印
            for (int i = 1; i <= numberOfPages; i++)
            {
                //waterMarkContent = pdfStamper.GetUnderContent(i);//内容下层加水印
                waterMarkContent = pdfStamper.GetOverContent(i);//内容上层加水印
                waterMarkContent.SetGState(gs);

                for (float left = 0; left < width; left += image1.Width)
                {
                    for (float top = 0; top < height; top += image1.Height)
                    {
                        iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance("C:\\WaterTemp.jpg");
                        image.GrayFill = 100;//透明度，灰色填充
                        image.Rotation = 45;//旋转
                        image.RotationDegrees = 45;//旋转角度
                        image.SetAbsolutePosition(left, height - image.Height - top);
                        Console.WriteLine(left + ":" + (height - image.Height - top));
                        waterMarkContent.AddImage(image);
                    }
                }
            }
            //strMsg = "success";
            //  return true;
        }
        catch (Exception ex)
        {
            throw ex;

        }
        finally
        {

            if (pdfStamper != null)
                pdfStamper.Close();

            if (pdfReader != null)
                pdfReader.Close();
        }
    }

    public static Image CreateWater(string words)
    {
        Bitmap bitmap = new Bitmap(595, 842);
        using (Graphics g = Graphics.FromImage(bitmap))
        {
            //重置图像
            g.ResetTransform();
            //设置旋转中心
            g.TranslateTransform(bitmap.Width / 2, bitmap.Height / 2);
            //旋转50度 顺时针
            g.RotateTransform(-50);
            //指定文本呈现的质量 解决文字锯齿问题
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            System.Drawing.Font font = new System.Drawing.Font("微软雅黑", 14);
            SolidBrush solidBrush = new SolidBrush(Color.FromArgb(100, 0, 0, 0));

            //设置文字、字体、大小、颜色、起始位置
            g.DrawString(words, font, solidBrush, new Point(-240, -100));

            g.DrawString(words, font, solidBrush, new Point(-340, 100));
        }

        //把水印图片保存在系统指定位置
        bitmap.Save("C:\\WaterTemp.jpg");

        iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance("C:\\WaterTemp.jpg");
        //透明度，灰色填充
        image.GrayFill = 20;
        image.SetAbsolutePosition(0, 0);

        return image;


    }
}
