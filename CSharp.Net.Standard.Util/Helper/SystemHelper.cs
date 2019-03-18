using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class SystemHelper
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
    /// 创建文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    public static string GetFilePath(string path, string file)
    {
        var filepath = Path.Combine(GetRealPath(path), file);

        if (!File.Exists(filepath))
        {
            File.Create(filepath).Dispose();
        }
        return filepath;
    }

}
