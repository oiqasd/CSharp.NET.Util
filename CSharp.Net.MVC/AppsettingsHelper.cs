using CSharp.Net.Util;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace CSharp.Net.Mvc;

/// <summary>
/// <code>
/// IConfigurationBuilder builder = new ConfigurationBuilder()
///                .AddEnvironmentVariables()
///                .SetBasePath(Directory.GetCurrentDirectory())
///                .AddJsonFile("appsettings.json", true, true);
/// string environmen = config_tmp["ASPNETCORE_ENVIRONMENT"];
/// if (!string.IsNullOrWhiteSpace(environmen))
/// {
///     builder = builder.AddEnvironmentVariables();
///     builder = builder.AddJsonFile($"appsettings.{environmen}.json", false, true);
/// }
/// builder = builder.AddCommandLine(args);
/// var config = builder.Build();
/// AppsettingsHelper.Init(config);
/// </code>
/// </summary>
public class AppsettingsHelper
{
    static IConfigurationRoot _configurationRoot;
    static IConfigurationSection _configurationSection;
    /// <summary>
    /// 在程序启动的时候加上
    /// </summary>
    /// <param name="args"></param>
    public static void Init(string[] args = null)
    {
        IConfigurationBuilder builder = new ConfigurationBuilder()
                       .AddEnvironmentVariables()
                       .SetBasePath(Directory.GetCurrentDirectory())
                       .AddJsonFile("appsettings.json", true, true);
        if (args != null) builder = builder.AddCommandLine(args);

        string environmen = builder.Build()["ASPNETCORE_ENVIRONMENT"];
        if (!string.IsNullOrWhiteSpace(environmen))
        {
            builder = builder.AddEnvironmentVariables();
            builder = builder.AddJsonFile($"appsettings.{environmen}.json", false, true);
        }
        //命令行最后加载，覆盖前面的参数
        if (args != null) builder = builder.AddCommandLine(args);
        var config = builder.Build();
        _configurationRoot = config;
        _configurationSection = config.GetSection("AppSetting");
    }
    /// <summary>
    /// 只支持在节点AppSetting下取一级参数的值
    /// </summary>
    /// <typeparam name="T">Type to cast result to</typeparam>
    /// <param name="key">Key in AppSettings section containing value</param>
    /// <returns>Converted type</returns>
    /// <exception cref="ConfigurationErrorsException">Thrown if key cannot be found or converted into requested type</exception>
    public static T AppSetting<T>(string key)
    {
        if (_configurationSection == null)
        {
            string msg = string.Format("未设置节点AppSetting");
            throw new Exception(msg);
            //throw new ConfigurationErrorsException(msg);
        }
        string configValue = _configurationSection[key];
        if (configValue == null)
        {
            string msg = string.Format("在AppSetting节点下未找到{0}", key);
            Utils.WriteLog(msg);
            //throw new ConfigurationErrorsException(msg);
        }
        try
        {
            return (T)Convert.ChangeType(configValue, typeof(T));
        }
        catch (Exception ex)
        {
            var msg = string.Format("类型转换失败 '{0}' to type of {1}", configValue, typeof(T).FullName);
            Utils.WriteLog(msg + "\n" + ex.GetExcetionMessage());
            throw new Exception(string.Format("类型转换失败 '{0}' to type of {1}", configValue, typeof(T).FullName), ex);
        }
    }

    /// <summary>
    /// 只支持在节点AppSetting下取一级参数的值
    /// </summary>
    /// <typeparam name="T">Type to cast result to</typeparam>
    /// <param name="key">Key in AppSettings section containing value</param>
    /// <param name="defaultValue">default to use if key is not found in configuration file</param>
    /// <returns>Converted type</returns>
    public static T AppSetting<T>(T defaultValue, string key)
    {
        T retValue = defaultValue;
        try
        {
            retValue = AppSetting<T>(key);
        }
        catch (Exception ex)
        {
            Utils.WriteLog("获取AppSetting失败" + "\n" + ex.GetExcetionMessage());
            //throw new ArgumentException("获取AppSetting失败");
        }
        return retValue;
    }

    /// <summary>
    /// 获取参数 如：GetConfigValue<int>(0,"AppSetting","AppInfo","AppId")
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="keys"></param>
    /// <returns></returns>
    public static T GetConfigValue<T>(params string[] keys)
    {
        if (_configurationRoot == null)
        {
            string msg = string.Format("未初始化ConfigHelper.Init()");
            throw new ArgumentException(msg);
        }
        string configValue = "";
        if (keys.Length == 1)
        {
            configValue = _configurationRoot[keys[0]];
        }
        else
        {
            IConfigurationSection section = _configurationRoot.GetSection(keys[0]);
            for (int i = 1; i < keys.Length - 1; i++)
            {
                section = section.GetSection(keys[i]);
            }
            configValue = section[keys[keys.Length - 1]];
        }
        return (T)Convert.ChangeType(configValue, typeof(T));
    }
    /// <summary>
    /// 获取参数 如：GetConfigValue<int>(0,"AppSetting","AppInfo","AppId")
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="keys"></param>
    /// <returns></returns>
    public static T GetConfigValue<T>(T defaultValue, params string[] keys)
    {
        T retValue = defaultValue;
        try
        {
            retValue = GetConfigValue<T>(keys);
        }
        catch (Exception ex)
        {
            Utils.WriteLog("获取参数失败" + "\n" + ex.GetExcetionMessage());
        }
        return retValue;
    }
}

