using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CSharp.Net.Standard.Util
{

    /// <summary>
    /// 应用程序帮助类
    /// </summary>
    public class AppDomainHelper
    {
        /// <summary>
        /// 获取运行根目录
        /// </summary>
        public static string GetRunRoot
        {
            get
            {
                return System.AppDomain.CurrentDomain.BaseDirectory;
            }
        }
    }

    public class SystemHelper
    {
        /// <summary>
        /// 获取用户数据缓存目录
        /// </summary>
        /// <returns></returns>
        public static string GetUserRoamingFolder(string path = "", bool defaultCreate = false)
        {
            string dir = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), path);
            if (defaultCreate)
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }

            return dir;
        }

    }
}