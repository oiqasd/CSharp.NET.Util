using System;
using System.Collections.Generic;
using System.Text;

public class PageList<T> : List<T>
{
    public PageList()
    {
        this.Page = new PageArgument();
        this.TotalCount = 0;
        this.AllPages = 0;
    }
    /// <summary>
    /// 请求量
    /// </summary>
    public PageArgument Page { get; set; }
    /// <summary>
    /// 总行数
    /// </summary>
    public int TotalCount { get; set; }
    /// <summary>
    /// 总页数
    /// </summary>
    public int AllPages { get; set; }

    public PageList(IEnumerable<T> items, PageArgument page, int totalCount)
    {
        if (totalCount != 0)
        {
            this.AddRange(items);
        }
        this.Page = page;
        this.TotalCount = totalCount;
        this.AllPages = (int)Math.Ceiling(totalCount / (double)page.PageSize);
    }
}
