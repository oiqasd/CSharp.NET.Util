using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// PageArgument扩展方法
/// </summary>
public static class PageArgumentExtentions
{
    /// <summary>
    /// 根据page对象生成Limit字符串_Mysql版
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    public static string ToMySqlLimitSql(this PageArgument page)
    {
        page = page ?? new PageArgument();
        return $" limit {page.PageSize * (page.PageIndex - 1)},{page.PageSize} ";
    }

    /// <summary>
    /// 当请求页码数大于最大页码数时，重置为最大页码数
    /// </summary>
    /// <param name="page">原分页参数</param>
    /// <param name="total">数据总条数</param>
    /// <returns>分页参数</returns>
    public static PageArgument ToCurrentPage(this PageArgument page, int total)
    {
        page = page ?? new PageArgument();

        // 获取最大页码数（向上取整）
        var maxPage = Convert.ToInt32(Math.Ceiling(total / (double)page.PageSize));

        // 若当前最大页码数小于前端传入请求页码数时，请求页码数重置为当前最大页码数
        if (maxPage < page.PageIndex)
        {
            page.PageIndex = maxPage;
        }

        return page;
    }
}
