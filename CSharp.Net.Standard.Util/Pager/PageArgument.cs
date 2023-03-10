namespace CSharp.Net.Standard.Util
{
    /// <summary>
    /// 请求分页参数
    /// </summary>
    public class PageArgument
    {
        private int index;
        private int size;
        /// <summary>
        /// 请求页码
        /// 默认1
        /// </summary>
        public int PageIndex
        {
            get
            {
                return index <= 0 ? 1 : index;
            }
            set
            {
                index = value;
            }
        }
        /// <summary>
        /// 每页记录数
        /// 默认10
        /// </summary>
        public int PageSize
        {
            get
            {
                return size <= 0 ? 10 : size;
            }
            set
            {
                size = value;
            }
        }
    }
}