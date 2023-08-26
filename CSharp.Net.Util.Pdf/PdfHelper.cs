using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CSharp.Net.Util.Pdf
{
    public class PdfHelper
    {
        /// <summary>
        /// 获取文件byte
        /// </summary>
        /// <param name="templatePath">模版路径</param>
        /// <param name="addWaterMark">是否添加水印</param>
        /// <param name="waterText">水印文本</param>
        /// <param name="printPara">填充表单数据</param>
        /// <returns></returns>
        public static byte[] GetPdfByte(string templatePath, bool addWaterMark = false, string waterText = "", Dictionary<string, string> printPara = null)
        {
            if (printPara == null)
            {
                var bts = File.ReadAllBytes(templatePath);
                return bts;
            }
            var fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"App_Data/Font/simsun.ttc");
            //获取中文字体，第三个参数表示为是否潜入字体，但只要是编码字体就都会嵌入。
            BaseFont baseFont = BaseFont.CreateFont($"{fontPath},1", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
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
                if (parameter.Key == "other_agreed" && !string.IsNullOrWhiteSpace(parameter.Value))
                {
                    int index = 1;
                    string tmp = parameter.Value;
                    int length = 43;
                    while (tmp.Length > length)
                    {
                        var key = $"{parameter.Key}{index}";
                        form.SetFieldProperty(key, "textfont", baseFont, null);
                        form.SetField(key, tmp.Substring(0, length));
                        tmp = tmp.Remove(0, length);
                        index++;
                    }
                    form.SetFieldProperty($"{parameter.Key}{index}", "textfont", baseFont, null);
                    form.SetField($"{parameter.Key}{index}", tmp);
                    continue;
                }
                form.SetField(parameter.Key, parameter.Value);
            }
            if (addWaterMark)
            {
                var pageSize = reader.NumberOfPages;
                PdfContentByte content;
                PdfGState gs = new PdfGState();
                iTextSharp.text.Rectangle psize = reader.GetPageSize(1);
                float width = psize.Width;
                float height = psize.Height;

                for (int i = 1; i <= pageSize; i++)
                {
                    content = stamp.GetOverContent(i);
                    //透明度
                    gs.FillOpacity = 0.3f;
                    content.SetGState(gs);
                    content.BeginText();
                    content.SetColorFill(BaseColor.GRAY);
                    content.SetFontAndSize(baseFont, 30);
                    content.SetTextMatrix(0, 0);
                    content.ShowTextAligned(Element.ALIGN_CENTER, waterText, width - 300, height - 200, 45);
                    content.ShowTextAligned(Element.ALIGN_CENTER, waterText, width - 300, height - 400, 45);
                    content.ShowTextAligned(Element.ALIGN_CENTER, waterText, width - 300, height - 600, 45);
                    content.EndText();
                }
            }
            stamp.Close();
            reader.Close();
            return stream.ToArray();
        }
    }
}
