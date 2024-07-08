using System.Collections.Generic;

namespace CSharp.Net.Mvc
{
    public class ResponsePaged<T> : Response
    {
        public ResponsePaged(IEnumerable<T> data) : base(ReturnCode.OK)
        {
            Data = data;
        }

        public ResponsePaged(PageList<T> pageData) : base(ReturnCode.OK)
        {
            Data = pageData.list;
            //this.PageIndex = pageData.PageIndex;
            //this.PageSize = pageData.PageSize;
            this.PageCount = pageData.TotalPages;
        }
        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; }
        /// <summary>
        /// 条数
        /// </summary>
        public int PageSize { get; }
        /// <summary>
        /// 页数
        /// </summary>
        public int PageCount { get; }

        /// <summary>
        /// 返回的数据，可以是Entity，也可以是List
        /// </summary>
        public IEnumerable<T> Data { get; set; }
    }
}
