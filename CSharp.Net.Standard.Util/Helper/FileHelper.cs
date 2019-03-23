using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CSharp.Net.Standard.Util
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
        /// <param name="fullName"></param>
        /// <returns></returns>
        public static string ReadFileText(string fullName)
        {
            if (!CheckFileExists(fullName))
            {
                return string.Empty;
            }

            return File.ReadAllText(fullName);
        }

        /// <summary>
        /// 覆盖文件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="contents"></param>
        public static void OverWrittenFile(string file, string contents)
        {
            //File.WriteAllText(fullName, contents); 
            using (System.IO.StreamWriter sw = new StreamWriter(file, false))
            { 
                sw.WriteLine(contents);
            }
        }

        /// <summary>
        /// 继续写入文件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="contents"></param>
        public static void AppendWrittenFile(string file, string contents)
        {
            //File.AppendAllText(fullName, contents + "\n");
            using (System.IO.StreamWriter sw = new StreamWriter(file, true))
            {
                sw.BaseStream.Seek(0, System.IO.SeekOrigin.End);
                sw.WriteLine(contents);
            }
        }
    }
}
