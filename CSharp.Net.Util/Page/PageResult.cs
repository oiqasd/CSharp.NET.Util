using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public static class PageResult
{
    /// <summary>
    /// 获取分页结果
    /// </summary>
    /// <typeparam name="T">数据item类型</typeparam>
    /// <param name="allItems">IQueryable对象</param>
    /// <param name="page">分页参数</param>
    /// <param name="pageIndexRange">超出最大页显示最后一页，默认false</param>
    /// <param name="sortModels">排序规则</param>
    /// <returns></returns>
    public static PageList<T> ToPageList<T>(this IQueryable<T> allItems, PageArgument page, bool pageIndexRange = false, IEnumerable<SortModel> sortModels = null)
    {
        page = page ?? new PageArgument();
        var itemIndex = (page.PageIndex - 1) * page.PageSize;
        var pageOfItems = allItems.OrderBy(sortModels).Skip(itemIndex).Take(page.PageSize);
        var totalItemCount = allItems.Count();

        if (totalItemCount <= 0)
            return new PageList<T>(page);

        if (pageIndexRange)
            page.ToCurrentPage(totalItemCount);
        return new PageList<T>(pageOfItems, page, totalItemCount);
    }

    /// <summary>
    /// 获取分页结果
    /// </summary>
    /// <typeparam name="T">数据item类型</typeparam>
    /// <param name="allItems">IQueryable对象</param>
    /// <param name="pageIndex">分页参数</param>
    /// <param name="pageSize">分页参数</param>
    /// <param name="pageIndexRange">超出最大页显示最后一页，默认false</param>
    /// <param name="sortModels">排序规则</param>
    /// <returns></returns>
    public static PageList<T> ToPageList<T>(this IQueryable<T> allItems, int pageIndex, int pageSize, bool pageIndexRange = false, IEnumerable<SortModel> sortModels = null)
    {
        var page = new PageArgument(pageIndex, pageSize);
        //var itemIndex = (page.PageIndex - 1) * page.PageSize;
        var pageOfItems = allItems.OrderBy(sortModels).Skip(page.ItemIndex).Take(page.PageSize);
        var totalItemCount = allItems.Count();

        if (totalItemCount <= 0)
            return new PageList<T>(page);

        if (pageIndexRange)
            page.ToCurrentPage(totalItemCount);
        return new PageList<T>(pageOfItems, page, totalItemCount);
    }

    /// <summary>
    /// 异步获取分页结果
    /// </summary>
    /// <typeparam name="T">数据item类型</typeparam>
    /// <param name="allItems">IQueryable对象</param>
    /// <param name="page">分页参数</param>
    /// <param name="pageIndexRange">超出最大页显示最后一页，默认false</param>
    /// <param name="sortModels">排序规则</param>
    /// <returns></returns>
    public static async Task<PageList<T>> ToPageListAsync<T>(this IQueryable<T> allItems, PageArgument page, bool pageIndexRange = false, IEnumerable<SortModel> sortModels = null)
    {
        var resultAsync = Task.Run(() =>
        {
            page = page ?? new PageArgument();
            var itemIndex = (page.PageIndex - 1) * page.PageSize;
            var pageOfItems = allItems.OrderBy(sortModels).Skip(itemIndex).Take(page.PageSize);
            var totalItemCount = allItems.Count();
            if (totalItemCount <= 0)
                return new PageList<T>(page);

            if (pageIndexRange)
                page.ToCurrentPage(totalItemCount);

            return new PageList<T>(pageOfItems, page, totalItemCount);
        });
        return await resultAsync;
    }


    /// <summary>
    /// 异步获取分页结果
    /// </summary>
    /// <typeparam name="T">数据item类型</typeparam>
    /// <param name="allItems">IQueryable对象</param>
    /// <param name="pageIndex">分页参数</param>
    /// <param name="pageSize">分页参数</param>
    /// <param name="pageIndexRange">超出最大页显示最后一页，默认false</param>
    /// <param name="sortModels">排序规则</param>
    /// <returns></returns>
    public static async Task<PageList<T>> ToPageListAsync<T>(this IQueryable<T> allItems, int pageIndex, int pageSize, bool pageIndexRange = false, IEnumerable<SortModel> sortModels = null)
    {
        var resultAsync = Task.Run(() =>
        {
            var page = new PageArgument(pageIndex, pageSize);
            var itemIndex = (page.PageIndex - 1) * page.PageSize;
            var pageOfItems = allItems.OrderBy(sortModels).Skip(itemIndex).Take(page.PageSize);
            var totalItemCount = allItems.Count();
            if (totalItemCount <= 0)
                return new PageList<T>(page);

            if (pageIndexRange)
                page.ToCurrentPage(totalItemCount);

            return new PageList<T>(pageOfItems, page, totalItemCount);
        });
        return await resultAsync;
    }

    /// <summary>
    /// 分页详情（带支持多字段排序）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="allItems"></param>
    /// <param name="paging"></param>
    /// <returns></returns>
    [Obsolete("暂时不用")]
    private static PageList<T> PageOutBy<T>(this IQueryable<T> allItems, PageArgument paging)
    {
        if (paging == null)
        {
            paging = new PageArgument();
        }

        paging.PageIndex = paging.PageIndex <= 0 ? 1 : paging.PageIndex;
        paging.PageSize = paging.PageSize <= 0 ? 10 : paging.PageSize;

        //if (!string.IsNullOrEmpty(paging.OrderBy) && paging.OrderBy.Contains(":"))
        //{
        //    string[] sort = new string[] { "asc", "desc" };
        //    int sortNum = 1;
        //    var orderList = paging.OrderBy.Split(',');
        //    foreach (var item in orderList)
        //    {
        //        var order = item.Split(':');
        //        if (order.Length != 2 || !sort.Contains(order[1].ToLower()))
        //        {
        //            continue;
        //        }

        //        var isAsc = order[1] == "asc" ? true : false;
        //        var property = typeof(T).GetProperties().FirstOrDefault(q => q.Name.ToLower() == order[0].ToLower());
        //        if (property == null)
        //        {
        //            continue;
        //        }
        //        if (sortNum > 1)
        //        {
        //            list = isAsc ? list.ThenBy(order[0]) : list.ThenByDescending(order[0]);
        //        }
        //        else
        //        {
        //            list = isAsc ? list.OrderBy(order[0]) : list.OrderByDescending(order[0]);
        //        }
        //        sortNum++;
        //    }
        //}

        //PageList<T> result = new PageList<T>()
        //{
        //    DataList = list.Skip((paging.PageIndex - 1) * paging.PageSize).Take(paging.PageSize).ToList(),
        //    PageIndex = paging.PageIndex,
        //    PageSize = paging.PageSize,
        //    OrderBy = paging.OrderBy,
        //    TotalCount = list.Count()
        //};
        var pageOfItems = allItems.Skip(paging.PageIndex).Take(paging.PageSize);
        var result = new PageList<T>(pageOfItems, paging, allItems.Count());
        return result;
    }
}
