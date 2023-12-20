using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Reflection;
using System.IO.MemoryMappedFiles;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Transactions;

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
        /// file to byte
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static byte[] GetBytes(string file)
        {
            using FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
            byte[] byteArray = new byte[fs.Length];
            fs.Read(byteArray, 0, byteArray.Length);
            return byteArray;
        }

        /// <summary>
        /// byte to file
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static void ByteToFile(byte[] bytes, string file)
        {
            bytes.SaveToFile(file);
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
            using (var fs = File.OpenRead(file))
            {
                byte[] b = new byte[fs.Length];
                fs.Read(b, 0, b.Length);
                return Encoding.GetEncoding(encoding).GetString(b);
            }

            /* 使用流读取
            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[1024];
                int bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length);
            }*/
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

            StreamReader reader = new StreamReader(file, Encoding.GetEncoding(encoding));
            while (!reader.EndOfStream)
                yield return await reader.ReadLineAsync();
        }

        /// <summary>
        /// 覆盖文件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="contents"></param>
        /// <param name="encoding">default:utf-8</param>
        public static async Task OverWrittenFile(string file, string contents, string encoding = "utf-8")
        {
            //File.WriteAllText(fullName, contents); 
            using (System.IO.StreamWriter sw = new StreamWriter(file, false, Encoding.GetEncoding(encoding)))
            {
                await sw.WriteLineAsync(contents);
            }
        }

        /// <summary>
        /// 继续写入文件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="contents"></param>
        /// <param name="encoding">default:utf-8</param>
        public static async Task AppendWrittenFile(string file, string contents, string encoding = "utf-8")
        {
            //File.AppendAllText(fullName, contents + "\n");
            using (System.IO.StreamWriter sw = new StreamWriter(file, true, Encoding.GetEncoding(encoding)))
            {
                sw.BaseStream.Seek(0, System.IO.SeekOrigin.End);
                await sw.WriteLineAsync(contents);
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
        /// 文件变化监控
        /// </summary>
        /// <param name="file">路径</param>
        /// <param name="action"></param>
        public static void Watcher(string file, Action<object, FileSystemEventArgs> action)
        {
            FileSystemWatcher watcher = new FileSystemWatcher(file);
            watcher.EnableRaisingEvents = true;
            watcher.Changed += (sender, e) => action(sender, e);
        }

        /// <summary>
        /// 文件内容比较
        /// </summary>
        /// <param name="sourse"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool Compare(string sourse, string target)
        {
            byte[] file1 = File.ReadAllBytes(sourse);
            byte[] file2 = File.ReadAllBytes(target);
            return file1.SequenceEqual(file2);
        }

        /// <summary>
        /// 设置文件只读属性
        /// </summary>
        /// <param name="file"></param>
        /// <param name="readOnly"></param>
        public static void SetReadOnly(string file, bool readOnly = true)
        {
            File.SetAttributes(file, readOnly ? FileAttributes.ReadOnly : FileAttributes.Normal);
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

        /*
        /// <summary>
        /// 修改自定义属性的属性值
        /// </summary>
        /// <param name="file">本地的文件</param>
        /// <param name="key">自定义的key</param>
        /// <returns>修改成功返回true，不成功返回false</returns>
        private static bool PropChange(OleDocumentProperties file, string key, string value)
        {
            //由于不能直接foreach，所以用了for循环
            for (int i = 0; i < file.CustomProperties.Count; i++)
            {
                if (file.CustomProperties[i].Name == key)
                {
                    file.CustomProperties[i].set_Value(value);//为指定自定义属性修改值
                    //file.CustomProperties[i].Remove();//删除
                    //file.Save();
                    return true;
                }
            }
            //不存在就添加
            file.CustomProperties.Add("TestKey", "TestValue");
            return false;
        }
        */

        /// <summary>
        /// 文件事务操作
        /// </summary>
        /// <param name="file"></param>
        void TransactionScopeDemo(string file)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                File.Move("old/path/file.txt", "new/path/file.txt");
                // 其他事务操作
                scope.Complete();
            }
        }

        /// <summary>
        /// 内存映射文件,适用大文件处理
        /// </summary>
        /// <param name="file"></param>
        void BigFileDemo(string file)
        {
            //创建内存映射文件 取消映射文件时，更改会自动传播到磁盘。
            //MemoryMappedFile.CreateOrOpen 也可创建共享内存
            using (MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(file, FileMode.Open))
            {
                long offset = 0x10000000; // 256 megabytes
                long length = 0x20000000; // 512 megabytes
                // 执行内存映射文件操作
                using (var accessor = mmf.CreateViewAccessor(offset, length))
                {
                    //accessor.Read(i, out val);
                    //accessor.Write(i, ref val);
                }
            }
        }

#if NET5_0_OR_GREATER
        /// <summary>
        /// 操作共享内存
        /// </summary>
        /// <param name="name"></param>
        /// <param name="data"></param>
        /// <param name="bSize">bytes,default:1024</param>
        [SupportedOSPlatform("windows")]
        public static async void WriteSharedMemory(string name, string data, int bSize = 1024)
        {
            using (MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen(name, bSize))
            {
                using (var stream = mmf.CreateViewStream())
                {
                    await stream.WriteAsync(Encoding.UTF8.GetBytes(data));
                }
            }
        }

        /// <summary>
        /// 读取共享内存
        /// </summary>
        /// <param name="name"></param>
        [SupportedOSPlatform("windows")]
        public static string ReadSharedMemory(string name)
        {
            using (MemoryMappedFile mmf = MemoryMappedFile.OpenExisting(name))
            {
                using (var stream = mmf.CreateViewStream())
                {
                    BinaryReader reader = new BinaryReader(stream);
                    return reader.ReadString();
                }
            }
        }
#endif
    }
}
