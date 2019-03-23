using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSharp.Net.Standard.Util
{
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
                return new PageList<T>();

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
            page = page ?? new PageArgument();
            var itemIndex = (page.PageIndex - 1) * page.PageSize;
            var pageOfItems = allItems.OrderBy(sortModels).Skip(itemIndex).Take(page.PageSize);
            var totalItemCount = allItems.Count();
            if (totalItemCount <= 0)
                return new PageList<T>();

            if (pageIndexRange)
                page.ToCurrentPage(totalItemCount);

            var resultAsync = Task.Run(() => { return new PageList<T>(pageOfItems, page, totalItemCount); });
            return await resultAsync;
        }
    }
}