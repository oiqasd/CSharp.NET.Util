using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
namespace CSharp.Net.Standard.Util
{
    [JsonObject(MemberSerialization.OptOut)]
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
        public PageList(IEnumerable<T> items, PageArgument page, int totalCount)
        {
            if (totalCount != 0)
            {
                this.AddRange(items);
                //this.List.AddRange(items);
            }
            this.Page = page;
            this.TotalCount = totalCount;
            this.AllPages = (int)Math.Ceiling(totalCount / (double)page.PageSize);
        }
    }
}