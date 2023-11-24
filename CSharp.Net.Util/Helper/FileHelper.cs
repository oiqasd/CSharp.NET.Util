using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace CSharp.Net.Util
{
    public class FileHelper
    {
        /// <summary>
        /// 返回路径，不存在则创建
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetRealPath(params string[] path)
        {
            var root = Path.Combine(path);
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }
            return root;
        }

        /// <summary>
        /// 返回文件路径
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="file">文件名</param>
        /// <param name="notExistsToCreate">是否自动创建,默认否</param>
        /// <returns></returns>
        public static string GetFilePath(string path, string file, bool notExistsToCreate = false)
        {
            var filepath = Path.Combine(GetRealPath(path), file);

            if (notExistsToCreate) CreateFile(filepath);
            return filepath;
        }

        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="fullName">文件名称</param>
        /// <param name="conver">是否覆盖</param>
        public static void CreateFile(string fullName, bool conver = false)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return;
            if (CheckFileExists(fullName))
            {
                if (conver) File.Delete(fullName);
                else return;
            }

            File.Create(fullName).Dispose();
        }

        /// <summary>
        /// 判断文件存在
        /// </summary>
        /// <param name="fullName"></param>
        /// <returns></returns>
        public static bool CheckFileExists(string fullName)
        {
            if (File.Exists(fullName))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 读取所有文本
        /// </summary>
        /// <param name="file"></param>
        /// <param name="encoding">default:utf-8</param>
        /// <returns></returns>
        public static string ReadFileText(string file, string encoding = "utf-8")
        {
            if (!CheckFileExists(file))
            {
                return string.Empty;
            }

            //return File.ReadAllText(fullName);
            using (var fs = System.IO.File.OpenRead(file))
            {
                byte[] b = new byte[fs.Length];
                fs.Read(b, 0, b.Length);
                return Encoding.GetEncoding(encoding).GetString(b);
            }
        }

        /// <summary>
        /// 读取所有行
        /// </summary>
        /// <param name="file"></param>
        /// <param name="encoding">default:utf-8</param>
        /// <returns></returns>
        public static async IAsyncEnumerable<string> ReadAllLines(string file, string encoding = "utf-8")
        {
            if (!CheckFileExists(file))
                yield break;

            using (Stream stream = File.OpenRead(file))
            {
                StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(encoding));
                while (!reader.EndOfStream)
                    yield return await reader.ReadLineAsync();
            }
        }

        /// <summary>
        /// 覆盖文件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="contents"></param>
        /// <param name="encoding">default:utf-8</param>
        public static void OverWrittenFile(string file, string contents, string encoding = "utf-8")
        {
            //File.WriteAllText(fullName, contents); 
            using (System.IO.StreamWriter sw = new StreamWriter(file, false, Encoding.GetEncoding(encoding)))
            {
                sw.WriteLine(contents);
            }
        }

        /// <summary>
        /// 继续写入文件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="contents"></param>
        /// <param name="encoding">default:utf-8</param>
        public static void AppendWrittenFile(string file, string contents, string encoding = "utf-8")
        {
            //File.AppendAllText(fullName, contents + "\n");
            using (System.IO.StreamWriter sw = new StreamWriter(file, true, Encoding.GetEncoding(encoding)))
            {
                sw.BaseStream.Seek(0, System.IO.SeekOrigin.End);
                sw.WriteLine(contents);
            }
        }

        /// <summary>
        /// 写入文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        /// <param name="text"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task WriteTextAsync(string filePath, string fileName, string text, CancellationToken cancellationToken)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(c.ToString(), "");

            try
            {
                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);
            }
            catch (NotSupportedException) { }

            await File.WriteAllTextAsync(path: Path.Combine(filePath, fileName), contents: text, cancellationToken);
        }

        /// <summary>
        /// 列表导出Csv格式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">数据列表</param>
        /// <param name="addTitle">是否添加首行标题，标题来源于字段名</param>
        /// <param name="encoding">默认urf8</param>
        /// <returns>可直接用于接口下载<para>例：return File(bytes, "application/octet-stream", "demo.csv")</para></returns>
        public static byte[] ExportToCsv<T>(List<T> list, bool addTitle = true, string encoding = "utf-8")
        {
            if (list.IsNullOrEmpty()) return null;
            StringBuilder sb = new StringBuilder();
            var properties = typeof(T).GetProperties();
            if (addTitle)
            {
                for (int i = 0; i < properties.Count(); i++)
                {
                    sb.Append(properties[i].Name);
                    if (i == properties.Count() - 1) continue;
                    sb.Append(",");
                }
            }
            foreach (var r in list)
            {
                sb.AppendLine();
                for (int i = 0; i < properties.Count(); i++)
                {
                    var value = properties[i].GetValue(r)?.ToString();
                    if (value != null && value.Contains(','))
                    {
                        value = $"\"{value}\"";
                    }
                    sb.Append(value);
                    if (i == properties.Count() - 1) continue;
                    sb.Append(",");
                }
            }
            return Encoding.GetEncoding(encoding).GetBytes(sb.ToString());
        }

        /// <summary>
        /// 修改文件属性信息
        /// 引用DSOFile.dll
        /// </summary>
        /// <param name="filePath"></param>
        void Modify(string filePath)
        {
        //    DSOFile.OleDocumentProperties dso = new DSOFile.OleDocumentProperties();

        //    dso.Open(filePath, false,
        //      DSOFile.dsoFileOpenOptions.dsoOptionOpenReadOnlyIfNoWriteAccess);

        //    dso.SummaryProperties.Title = "标题";
        //    dso.SummaryProperties.Subject = "主题";
        //    dso.SummaryProperties.Company = "公司";
        //    dso.SummaryProperties.Author = "作者";
        //    dso.SummaryProperties.Category = "类别";
        //    dso.SummaryProperties.Comments = "备注";
        //    dso.SummaryProperties.LastSavedBy = "最后一次保存者";
        //    dso.SummaryProperties.Manager = "管理者";
        //    dso.SummaryProperties.Keywords = "标记";

        //    //PropChange(dso, "TestKey", "TestValue");
        //    dso.Save();
        //    dso.Close(false);

            File.SetAttributes(filePath, FileAttributes.Normal);
            File.SetCreationTime(filePath, DateTime.Now);//创建时间
            File.SetLastWriteTime(filePath, DateTime.Now);//修改时间
            File.SetLastAccessTime(filePath, DateTime.Now);//访问时间
        }

        ///// <summary>
        ///// 修改自定义属性的属性值
        ///// </summary>
        ///// <param name="file">本地的文件</param>
        ///// <param name="key">自定义的key</param>
        ///// <returns>修改成功返回true，不成功返回false</returns>
        //private static bool PropChange(OleDocumentProperties file, string key, string value)
        //{
        //    //由于不能直接foreach，所以用了for循环
        //    for (int i = 0; i < file.CustomProperties.Count; i++)
        //    {
        //        if (file.CustomProperties[i].Name == key)
        //        {
        //            file.CustomProperties[i].set_Value(value);//为指定自定义属性修改值
        //            //file.CustomProperties[i].Remove();//删除
        //            //file.Save();
        //            return true;
        //        }
        //    }
        //    //不存在就添加
        //    file.CustomProperties.Add("TestKey", "TestValue");
        //    return false;
        //}
    }
}
