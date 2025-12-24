using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 分页
/// </summary>
/// <typeparam name="T"></typeparam>
public class PageList<T>
{
    public PageList() { }
    
    /// <summary>
    /// 实例化PageList
    /// </summary>
    /// <param name="page"></param>
    /// <param name="initList">list是否实例化，默认false</param>
    public PageList(PageArgument page, bool initList = false)
    {
        TotalCount = 0;
        PageIndex = page?.PageIndex ?? 0;
        PageSize = page?.PageSize ?? 0;
        if (initList)
            list = new List<T>();
    }

    /// <summary>
    /// 实例化PageList
    /// </summary>
    /// <param name="initList">list是否实例化</param>
    public PageList(bool initList = false)
    {
        if (initList)
            list = new List<T>();
    }

    //protected PageArgument page;

    /// <summary>
    /// 请求页码
    /// 默认1
    /// </summary>
    public int PageIndex { get; set; }
    /// <summary>
    /// 每页记录数
    /// 默认10
    /// </summary>
    public int PageSize { get; set; }
    /*
       /// <summary>
       /// 是否有下一页
       /// </summary>
       public bool HasNextPages { get { return PageIndex < TotalPages; } }
       /// <summary>
       /// 是否有上一页
       /// </summary>
       public bool HasPrevPages { get { return PageIndex - 1 > 0; ; } }
    */

    /// <summary>
    /// 总行数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages { get { return (int)Math.Ceiling(TotalCount / (double)PageSize); } }

    /// <summary>
    /// 数据列表
    /// </summary>
    public List<T> list { get; set; }

    /// <summary>
    /// 列表
    /// </summary>
    /// <param name="items"></param>
    /// <param name="pageIndex">default -1</param>
    /// <param name="pageSize">default -1</param>
    /// <param name="totalCount">default -1</param>
    public PageList(IEnumerable<T> items, int totalCount = -1, int pageIndex = -1, int pageSize = -1)
    {
        if (totalCount != 0 && items != null)
        {
            this.list = items.ToList();
            //this.List.AddRange(items);
        }
        this.TotalCount = totalCount;
        PageIndex = pageIndex;
        PageSize = pageSize;
    }

    /// <summary>
    /// 列表
    /// </summary>
    /// <param name="items"></param>
    /// <param name="page"></param>
    /// <param name="totalCount">default -1</param>
    public PageList(IEnumerable<T> items, PageArgument page, int totalCount = -1)
    {
        if (totalCount != 0 && items != null)
        {
            this.list = items.ToList();
            //this.List.AddRange(items);
        }
        this.TotalCount = totalCount;
        PageIndex = page.PageIndex;
        PageSize = page.PageSize;
    }
}


/*
/// <summary>
/// autoMapper无法映射，故重新做
/// </summary>
/// <typeparam name="T"></typeparam>
//[JsonObject(MemberSerialization.OptOut)]
class PageList<T> : List<T>
{
    public PageList1()
    {
        this.Page = new PageArgument();
        this.TotalCount = 0;
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
    public int AllPages { get { return Page == null ? 0 : (int)Math.Ceiling(TotalCount / (double)Page.PageSize); } }

    /// <summary>
    /// 数据列表
    /// </summary>
    public List<T> list { get { return this.GetRange(0, this.Count); } }//json序列号加上OptOut会排除自身列表，不加会排序其它的属性，故通过这个中转
    /// <summary>
    /// 列表
    /// </summary>
    /// <param name="items"></param>
    /// <param name="page"></param>
    /// <param name="totalCount"></param>
    public PageList1(IEnumerable<T> items, PageArgument page, int totalCount)
    {
        if (totalCount != 0)
        {
            this.AddRange(items);
            //this.List.AddRange(items);
        }
        this.Page = page;
        this.TotalCount = totalCount;
        //this.AllPages = (int)Math.Ceiling(totalCount / (double)page.PageSize);
    }
}

*/