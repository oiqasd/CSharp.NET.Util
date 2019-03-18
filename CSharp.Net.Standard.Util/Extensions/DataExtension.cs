using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

/// <summary>
/// 数据集扩展
/// </summary>
public static class DataExtension
{
    /// <summary>
    /// 判断DS是否有Table
    /// </summary>
    /// <param name="ds"></param>
    /// <returns></returns>
    public static bool HaveTable(this DataSet ds)
    {
        if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 获取第n个Table
    /// </summary>
    /// <param name="index">索引</param>
    /// <returns></returns>
    public static DataTable GetTable(this DataSet ds, int index)
    {
        if (ds.HaveTable())
        {
            if (ds.Tables.Count > index)
            {
                if (ds.Tables[index].IsNotEmpty())
                {
                    return ds.Tables[index];
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 检查Table是否为空
    /// dt.IsNotEmpty()
    /// </summary>
    /// <returns></returns>
    public static bool IsNotEmpty(this DataTable dt)
    {
        if (dt != null && dt.Rows.Count > 0)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 获取第n行数据
    /// 如 ds.GetTable(0).GetRow(0)
    /// </summary>
    /// <param name="index">索引</param>
    /// <returns></returns>
    public static DataRow GetRow(this DataTable dt, int index)
    {
        if (dt.IsNotEmpty() && dt.Rows.Count > index)
        {
            return dt.Rows[index];
        }
        return null;
    }

}
