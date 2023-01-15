using CSharp.Net.Util;
using System;
using System.Collections.Generic;
using System.Text;

public static class ObjectExtension
{
    /// <summary>
    /// 获取对象的属性值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="Obj"></param>
    /// <param name="colum"></param>
    /// <returns></returns>
    public static T GetProperty<T>(this object Obj, string colum)
    {
        return GetProperty<T>(Obj, colum, default(T));
    }
    public static T GetProperty<T>(this object Obj, string colum, T defaultValue)
    {
        if (Obj == null)
            return defaultValue;

        object obj;

        try
        {
            obj = Obj.GetType().GetProperty(colum).GetValue(Obj, null);
        }
        catch (NullReferenceException e)
        {
            throw new Exception(string.Format("{0} The {1} does not exist in {2}", e.Message, colum, Obj.GetType().Name));
        }
        return ConvertHelper.ConvertTo<T>(obj);
    }
    public static T GetProperty<T>(this object Obj, string colum, T defaultValue, string abc)
    {
        return GetProperty<T>(Obj, colum, default(T));
    }
}