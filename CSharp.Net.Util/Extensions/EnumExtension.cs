using CSharp.Net.Standard.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

/// <summary>
/// 枚举扩展方法
/// </summary>
public static class EnumExtension
{

    #region 获取枚举的Description
    /// <summary>
    /// 扩展方法，获得枚举的Description
    /// </summary>
    /// <param name="value">枚举值</param>        
    /// <returns>枚举的Description</returns>
    public static string GetDescription(this Enum value)
    {
        Type type = value.GetType();
        string name = Enum.GetName(type, value);
        if (string.IsNullOrEmpty(name))
        {
            return string.Empty;
        }
        FieldInfo field = type.GetField(name);
        DescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

        return attribute == null ? name : attribute.Description;
    }

    /// <summary>
    /// 使用string.format对Description赋值
    /// xxx{0}xxx
    /// </summary>
    /// <param name="value"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    public static string GetDescription(this Enum value, params object[] param)
    {
        Type type = value.GetType();
        string name = Enum.GetName(type, value);
        if (string.IsNullOrEmpty(name))
        {
            return string.Empty;
        }
        FieldInfo field = type.GetField(name);
        DescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

        return attribute == null ? name : string.Format(attribute.Description, param);
    }

    /// <summary>
    /// 获取枚举英文描述
    /// </summary>
    /// <param name="value">枚举</param>
    /// <returns></returns>
    public static string GetEDescription(this Enum value)
    {
        Type type = value.GetType();
        var orders = type.GetField(value.ToString()).GetCustomAttributes(typeof(EDescriptionAttribute), false);
        if (orders.Length > 0)
        {
            var attribute = orders[0] as EDescriptionAttribute;
            if (attribute != null) return attribute.EDescription;
        }
        return string.Empty;
    }
    #endregion

    #region 

    #endregion

    /// <summary>
    /// 通过枚举类型和枚举返回对应的序号
    /// </summary>
    /// <param name="value">枚举</param>
    /// <returns></returns>
    public static int GetOrder(this Enum value)
    {
        Type type = value.GetType();
        var orders = type.GetField(value.ToString()).GetCustomAttributes(typeof(EnumOrderAttribute), false);
        if (orders.Length > 0)
        {
            var attribute = orders[0] as EnumOrderAttribute;
            if (attribute != null) return attribute.EnumOrder;
        }
        return 0;
    }

}

