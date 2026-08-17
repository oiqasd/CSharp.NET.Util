using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CSharp.Net.Tools
{
    public class ImageUtils
    {
        /// <summary>
        /// 获取图片尺寸
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static async Task<ImageSize> GetImageSizeAsync(Stream stream)
        {
            if (stream.CanSeek && stream.Position != 0)
                stream.Position = 0;

            var dimensions = await Image.IdentifyAsync(stream);
            if (dimensions == null)
                throw new InvalidOperationException("无法识别的图片格式或数据已损坏。");
            return new ImageSize() { Width = dimensions.Width, Height = dimensions.Height };
             
            //SixLabors.ImageSharp.Configuration.Default.MemoryAllocator.ReleaseRetainedResources();
        }

        /// <summary>
        /// 表格转图片
        /// </summary>
        /// <param name="headers">new List<string> { "标题1", "标题2" }</param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] TableToImage(List<string> headers, List<List<string>> data)
        {
            // 准备表格数据
            //var headers = new List<string> { "产品名称", "销量（件）", "销售额（元）" };
            //var data = new List<List<string>>{
            //             new List<string> { "手机", "500", "300000" },
            //             new List<string> { "电脑", "200", "800000" },
            //             new List<string> { "电视", "300", "450000" }
            //            };

            // 字体文件路径（需自行下载中文字体，如 simhei.ttf，放在项目目录下）
            string fontPath = Path.Combine(AppContext.BaseDirectory, "ShanHaiJiGuSongKe-JianFan-2.ttf");

            // 转换为图片
            var converter = new TableToImageConverter();
            var imageBytes = converter.ConvertToImage(headers, data, fontPath);

            return imageBytes;
            // 保存图片到本地（测试用）
            //  File.WriteAllBytes("table.png", imageBytes);
        }
    }
}
