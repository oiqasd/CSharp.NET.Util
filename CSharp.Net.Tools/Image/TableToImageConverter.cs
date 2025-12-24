using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CSharp.Net.Tools
{

    public class demo
    {
        public byte[] TableToImageConverterTest()
        {

            // 准备表格数据
            var headers = new List<string> { "产品名称", "销量（件）", "销售额（元）" };
            var rows = new List<List<string>>{
                         new List<string> { "手机", "500", "300000" },
                         new List<string> { "电脑", "200", "800000" },
                         new List<string> { "电视", "300", "450000" }
                        };

            // 字体文件路径（需自行下载中文字体，如 simhei.ttf，放在项目目录下）
            string fontPath = Path.Combine(AppContext.BaseDirectory, "ShanHaiJiGuSongKe-JianFan-2.ttf");

            // 转换为图片
            var converter = new TableToImageConverter();
            var imageBytes = converter.ConvertToImage(headers, rows, fontPath);

            return imageBytes;
            // 保存图片到本地（测试用）
            //  File.WriteAllBytes("table.png", imageBytes);
        }

    }
    /// <summary>
    /// 表格转图片
    /// </summary>
    public class TableToImageConverter
    {
        private int cellPadding = 10;//单元格边距
        private int imageMargin = 5;//图片边缘空白

        /// <summary>
        /// 将表格数据转换为图片并返回
        /// </summary>
        public byte[] ConvertToImage(List<string> headers, List<List<string>> rows, string fontPath)
        {
            // 配置表格参数
            int borderWidth = 1;
            int fontSize = 16;

            // 加载字体
            FontCollection fonts = new FontCollection();
            FontFamily fontFamily = fonts.Add(fontPath);
            Font font = fontFamily.CreateFont(fontSize, FontStyle.Regular);

            // 计算每列的最大宽度
            List<float> columnWidths = CalculateColumnWidths(headers, rows, font);

            // 计算图片总尺寸
            int totalWidth = (int)columnWidths.Sum() + (columnWidths.Count + 1) * borderWidth + 2 * imageMargin;
            int rowHeight = (int)(font.Size * 2);
            int totalHeight = (rows.Count + 1) * rowHeight + (rows.Count + 2) * borderWidth + 2 * imageMargin;

            using (Image<Rgba32> image = new Image<Rgba32>(totalWidth, totalHeight, Color.White.ToPixel<Rgba32>()))
            {
                int currentY = imageMargin + borderWidth;

                // 绘制表头
                DrawRow(image, headers, font, columnWidths, ref currentY, rowHeight, cellPadding, borderWidth, Color.LightGray.ToPixel<Rgba32>());

                // 绘制内容行
                foreach (var row in rows)
                {
                    DrawRow(image, row, font, columnWidths, ref currentY, rowHeight, cellPadding, borderWidth, Color.White.ToPixel<Rgba32>());
                }

                // 保存图片为字节数组
                using (MemoryStream ms = new MemoryStream())
                {
                    image.SaveAsPng(ms);
                     return ms.ToArray(); 
                }
            }
        }

        /// <summary>
        /// 计算每列的最大宽度
        /// </summary>
        private List<float> CalculateColumnWidths(List<string> headers, List<List<string>> rows, Font font)
        {
            List<float> columnWidths = new List<float>();

            // 计算表头的列宽
            for (int i = 0; i < headers.Count; i++)
            {
                // 使用正确的文本测量方法和参数
                float width = TextMeasurer.MeasureBounds(headers[i], new TextOptions(font)).Width;
                columnWidths.Add(width);
            }

            // 用内容行的宽度更新列宽
            foreach (var row in rows)
            {
                for (int i = 0; i < row.Count; i++)
                {
                    if (i >= columnWidths.Count) continue;

                    float width = TextMeasurer.MeasureBounds(row[i], new TextOptions(font)).Width;
                    if (width > columnWidths[i])
                    {
                        columnWidths[i] = width;
                    }
                }
            }

            // 加上单元格内边距
            for (int i = 0; i < columnWidths.Count; i++)
            {
                columnWidths[i] += 2 * cellPadding;
            }

            return columnWidths;
        }

        /// <summary>
        /// 绘制一行表格
        /// </summary>
        private void DrawRow(Image<Rgba32> image, List<string> cells, Font font, List<float> columnWidths,
                     ref int currentY, int rowHeight, int cellPadding, int borderWidth, Rgba32 bgColor)
        {
            int currentX = imageMargin + borderWidth;
            int rowTop = currentY; // 保存当前Y坐标作为行的顶部位置

            // 绘制所有单元格背景和内容
            for (int i = 0; i < cells.Count; i++)
            {
                if (i >= columnWidths.Count) continue;

                // 绘制单元格背景（使用临时变量保存位置和尺寸）
                var cellRect = new RectangleF(currentX, rowTop, columnWidths[i], rowHeight);
                image.Mutate(ctx => ctx.Fill(bgColor, cellRect));

                var textPosition = new PointF(currentX + columnWidths[i] / 2, rowTop + rowHeight / 2);
                // 绘制单元格文本
                var textOptions = new TextOptions(font)
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                RichTextOptions richTextOptions = new RichTextOptions(font)
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Origin = textPosition
                };

                image.Mutate(ctx => ctx.DrawText(richTextOptions, cells[i], Color.Black));

                // 绘制单元格右边框
                var borderEnd = new PointF(currentX + columnWidths[i], rowTop + rowHeight);
                image.Mutate(ctx => ctx.DrawLine(
                    Color.Black,
                    borderWidth,
                    new PointF(currentX + columnWidths[i], rowTop),
                    borderEnd
                ));

                currentX += (int)columnWidths[i] + borderWidth;
            }

            // 绘制行底边框（使用保存的行顶部位置）
            var bottomBorderY = rowTop + rowHeight;
            image.Mutate(ctx => ctx.DrawLine(
                Color.Black,
                borderWidth,
                new PointF(imageMargin, bottomBorderY),
                new PointF(image.Width - imageMargin, bottomBorderY)
            ));

            // 在lambda表达式外部修改ref参数
            currentY = rowTop + rowHeight + borderWidth;
        }
    }
}
