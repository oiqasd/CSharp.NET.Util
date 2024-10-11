using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace CSharp.Net.Util
{
    public static class ZipFileHelper
    {
        /// <summary>
        /// 解压
        /// </summary>
        /// <param name="zipFileName"></param>
        /// <param name="directory">输出目录</param>
        /// <returns></returns>
        public static IList<string> ExtractFilesFromZip(string zipFileName, string directory)
        {
            List<string> list = new List<string>();
            using (ZipArchive archive = ZipFile.OpenRead(zipFileName))
            {
                //archive.ExtractToDirectory(directory);
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (!IsDirectory(entry))
                    {
                        string path = Path.Combine(directory, entry.FullName);
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                        entry.ExtractToFile(path, true);
                        list.Add(path);
                    }
                }
            }
            return list;
        }

        private static bool IsDirectory(ZipArchiveEntry entry) =>
            entry.FullName.EndsWith("/") && entry.Name.Equals(string.Empty);
    }

}
