using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Linq;

/// <summary>
/// 分页
/// </summary>
/// <typeparam name="T"></typeparam>
public class PageList<T>
{
    public PageList(PageArgument page = null)
    {
        page = page ?? new PageArgument();
        this.TotalCount = 0;
        PageIndex = page.PageIndex;
        PageSize = page.PageSize;
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

    /// <summary>
    /// 总行数
    /// </summary>
    public int TotalCount { get; set; }
    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages { get { return (int)Math.Ceiling(TotalCount / (double)PageSize); } }
    /// <summary>
    /// 是否有下一页
    /// </summary>
    public bool HasNextPages { get { return PageIndex < TotalPages; } }
    /// <summary>
    /// 是否有上一页
    /// </summary>
    public bool HasPrevPages { get { return PageIndex - 1 > 0; ; } }
    /// <summary>
    /// 数据列表
    /// </summary>
    public List<T> list { get; set; }

    /// <summary>
    /// 列表
    /// </summary>
    /// <param name="items"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="totalCount"></param>
    public PageList(IEnumerable<T> items, int pageIndex, int pageSize, int totalCount)
    {
        if (totalCount != 0 && items != null)
        {
            this.list = items.ToList();
            //this.List.AddRange(items);
        }
        this.TotalCount = totalCount;
        PageIndex = pageIndex;
        PageSize = pageSize;
        //this.AllPages = (int)Math.Ceiling(totalCount / (double)page.PageSize);
    }

    /// <summary>
    /// 列表
    /// </summary>
    /// <param name="items"></param>
    /// <param name="page"></param>
    /// <param name="totalCount"></param>
    public PageList(IEnumerable<T> items, PageArgument page, int totalCount)
    {
        if (totalCount != 0 && items != null)
        {
            this.list = items.ToList();
            //this.List.AddRange(items);
        }
        page = page ?? new PageArgument();
        this.TotalCount = totalCount;
        PageIndex = page.PageIndex;
        PageSize = page.PageSize;

        //this.AllPages = (int)Math.Ceiling(totalCount / (double)page.PageSize);
    }
}



///// <summary>
///// autoMapper无法映射，故重新做
///// </summary>
///// <typeparam name="T"></typeparam>
////[JsonObject(MemberSerialization.OptOut)]
//class PageList1<T> : List<T>
//{
//    public PageList1()
//    {
//        this.Page = new PageArgument();
//        this.TotalCount = 0;
//    }
//    /// <summary>
//    /// 请求量
//    /// </summary> 
//    public PageArgument Page { get; set; }
//    /// <summary>
//    /// 总行数
//    /// </summary>
//    public int TotalCount { get; set; }
//    /// <summary>
//    /// 总页数
//    /// </summary>
//    public int AllPages { get { return Page == null ? 0 : (int)Math.Ceiling(TotalCount / (double)Page.PageSize); } }

//    /// <summary>
//    /// 数据列表
//    /// </summary>
//    public List<T> list { get { return this.GetRange(0, this.Count); } }//json序列号加上OptOut会排除自身列表，不加会排序其它的属性，故通过这个中转
//    /// <summary>
//    /// 列表
//    /// </summary>
//    /// <param name="items"></param>
//    /// <param name="page"></param>
//    /// <param name="totalCount"></param>
//    public PageList1(IEnumerable<T> items, PageArgument page, int totalCount)
//    {
//        if (totalCount != 0)
//        {
//            this.AddRange(items);
//            //this.List.AddRange(items);
//        }
//        this.Page = page;
//        this.TotalCount = totalCount;
//        //this.AllPages = (int)Math.Ceiling(totalCount / (double)page.PageSize);
//    }
//}

