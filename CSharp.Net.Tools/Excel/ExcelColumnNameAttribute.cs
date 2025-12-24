using System;

namespace CSharp.Net.Tools
{
    public class ExcelColumnNameAttribute : Attribute
    {
        /// <summary>
        /// 导入/导出标签
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// 字段序号
        /// </summary>
        public int Sort { get; set; }
        public ExcelColumnNameAttribute(string columnname)
        {
            ColumnName = columnname;
        }

        public ExcelColumnNameAttribute(string columnname, int sort)
        {
            ColumnName = columnname;
            Sort = sort;
        }
    }
}
