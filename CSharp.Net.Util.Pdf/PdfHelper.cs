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
    }
}
