using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 实体类转换类
    /// ConvertHelper也有类似功能
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ModelHelpler<T> where T : class, new()
    {
        #region DataTable转换成实体类

        /// <summary>
        /// 填充对象列表：用DataSet的第一个表填充实体类
        /// </summary>
        /// <param name="ds">DataSet</param>
        /// <returns></returns>
        public List<T> FillModel(DataSet ds)
        {
            return FillModel(ds, false);
        }

        /// <summary>  
        /// 填充对象列表：用DataSet的第index个表填充实体类
        /// </summary>  
        public List<T> FillModel(DataSet ds, int index)
        {
            return FillModel(ds, index, false);
        }

        /// <summary>  
        /// 填充对象列表：用DataTable填充实体类
        /// </summary>  
        public List<T> FillModel(DataTable dt)
        {
            return FillModel(dt, false);
        }

        /// <summary>  
        /// 填充对象：用DataRow填充实体类
        /// </summary>  
        public T FillModel(DataRow dr)
        {
            return FillModel(dr, false);
        }

        #region 重载方法
        /// <summary>
        /// 填充对象列表：用DataSet的第一个表填充实体类
        /// </summary>
        /// <param name="ds">DataSet</param>
        /// <param name="IsConvert">是否进行数据处理 true 处理</param>
        /// <returns></returns>
        public List<T> FillModel(DataSet ds, bool IsConvert)
        {
            if (ds == null || ds.Tables[0] == null || ds.Tables[0].Rows.Count == 0)
            {
                return null;
            }
            else
            {
                return FillModel(ds.Tables[0], IsConvert);
            }
        }

        /// <summary>  
        /// 填充对象列表：用DataSet的第index个表填充实体类
        /// </summary>  
        /// <param name="IsConvert">是否进行数据处理 true 处理</param>
        /// <returns></returns>
        public List<T> FillModel(DataSet ds, int index, bool IsConvert)
        {
            if (ds == null || ds.Tables.Count <= index || ds.Tables[index].Rows.Count == 0)
            {
                return null;
            }
            else
            {
                return FillModel(ds.Tables[index], IsConvert);
            }
        }

        /// <summary>  
        /// 填充对象：用DataRow填充实体类
        /// </summary>  
        /// <param name="dr">行数据</param>
        /// <param name="IsConvert">是否进行数据处理 true 处理</param>
        /// <returns></returns>
        public T FillModel(DataRow dr, bool IsConvert)
        {
            if (dr == null)
            {
                return default(T);
            }

            T model = new T();

            for (int i = 0; i < dr.Table.Columns.Count; i++)
            {
                var columnValue = dr[i];
                var columnInfo = dr.Table.Columns[i];
                PropertyInfo propertyInfo = model.GetType().GetProperty(columnInfo.ColumnName);
                if (propertyInfo != null && dr[i] != DBNull.Value)
                {
                    if (IsConvert)
                    {
                        if (columnInfo.DataType == typeof(decimal))
                        {
                            decimal d = Math.Ceiling(Convert.ToDecimal(dr[i]) * 100) / 100;
                            SetPropValue(model, propertyInfo, Math.Round(d, 2));
                        }
                        else
                            SetPropValue(model, propertyInfo, columnValue);
                    }
                    else
                        SetPropValue(model, propertyInfo, columnValue);
                }
            }
            return model;
        }


        /// <summary>  
        /// 填充对象列表：用DataTable填充实体类
        /// </summary>  
        /// <param name="IsConvert">是否进行数据处理 true 处理</param>
        /// <returns></returns>
        public List<T> FillModel(DataTable dt, bool IsConvert)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                return null;
            }
            List<T> modelList = new List<T>();
            foreach (DataRow dr in dt.Rows)
            {
                T model = new T();
                for (int i = 0; i < dr.Table.Columns.Count; i++)
                {
                    var columnValue = dr[i];
                    var columnInfo = dr.Table.Columns[i];
                    PropertyInfo propertyInfo = model.GetType().GetProperty(columnInfo.ColumnName);
                    if (propertyInfo != null && columnValue != DBNull.Value)
                    {
                        if (IsConvert)
                        {
                            if (columnInfo.DataType == typeof(decimal))
                            {
                                decimal d = Math.Ceiling(Convert.ToDecimal(columnValue) * 100) / 100;
                                SetPropValue(model, propertyInfo, Math.Round(d, 2));
                            }
                            else
                                SetPropValue(model, propertyInfo, columnValue);
                        }
                        else
                            SetPropValue(model, propertyInfo, columnValue);
                    }
                }

                modelList.Add(model);
            }
            return modelList;
        }

        private void SetPropValue(T model, PropertyInfo propertyInfo, object column)
        {
            if (propertyInfo.PropertyType == typeof(bool))
            {
                propertyInfo.SetValue(model, ConvertHelper.ConvertTo<bool>(column), null);
            }
            else if (propertyInfo.PropertyType == typeof(DateTime?))
            {
                propertyInfo.SetValue(model, ConvertHelper.ConvertTo<DateTime>(column), null);
            }
            else if (propertyInfo.PropertyType == typeof(DateTime))
            {
                propertyInfo.SetValue(model, ConvertHelper.ConvertTo<DateTime>(column), null);
            }
            else if (propertyInfo.PropertyType == typeof(decimal))
            {
                propertyInfo.SetValue(model, ConvertHelper.ConvertTo<decimal>(column), null);
            }
            else if (propertyInfo.PropertyType == typeof(int))
            {
                propertyInfo.SetValue(model, ConvertHelper.ConvertTo<int>(column), null);
            }
            else if (propertyInfo.PropertyType == typeof(long))
            {
                propertyInfo.SetValue(model, ConvertHelper.ConvertTo<long>(column), null);
            }
            else if (propertyInfo.PropertyType == typeof(double))
            {
                propertyInfo.SetValue(model, ConvertHelper.ConvertTo<double>(column), null);
            }
            else
            {
                propertyInfo.SetValue(model, column, null);
            }
        }
        #endregion

        #endregion

        #region 实体类转换成DataTable

        /// <summary>
        /// 实体类转换成DataSet
        /// </summary>
        /// <param name="modelList">实体类列表</param>
        /// <returns></returns>
        public DataSet FillDataSet(List<T> modelList)
        {
            if (modelList == null || modelList.Count == 0)
            {
                return null;
            }
            else
            {
                DataSet ds = new DataSet();
                ds.Tables.Add(FillDataTable(modelList));
                return ds;
            }
        }

        /// <summary>
        /// 实体类转换成DataTable
        /// </summary>
        /// <param name="modelList">实体类列表</param>
        /// <returns></returns>
        public DataTable FillDataTable(List<T> modelList)
        {
            if (modelList == null || modelList.Count == 0)
            {
                return null;
            }
            DataTable dt = CreateData(modelList[0]);

            foreach (T model in modelList)
            {
                DataRow dataRow = dt.NewRow();
                foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
                {
                    dataRow[propertyInfo.Name] = propertyInfo.GetValue(model, null);
                }
                dt.Rows.Add(dataRow);
            }
            return dt;
        }

        /// <summary>
        /// 根据实体类得到表结构
        /// </summary>
        /// <param name="model">实体类</param>
        /// <returns></returns>
        private DataTable CreateData(T model)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
            {
                dataTable.Columns.Add(new DataColumn(propertyInfo.Name, propertyInfo.PropertyType));
            }
            return dataTable;
        }

        #region 根据字段返回唯一数据
        /// <summary>
        /// 根据字段返回唯一数据
        /// </summary>
        /// <param name="SourceTable"></param>
        /// <param name="FieldNames"></param>
        /// <returns></returns>
        public DataTable SelectDistinctDataTable(DataTable SourceTable, params string[] FieldNames)
        {
            if (SourceTable == null || SourceTable.Rows.Count == 0 || FieldNames == null || FieldNames.Length == 0)
                return null;

            object[] lastValues;
            DataTable newTable;
            DataRow[] orderedRows;

            lastValues = new object[FieldNames.Length];
            newTable = SourceTable.Clone();

            orderedRows = SourceTable.Select("", string.Join(",", FieldNames));

            foreach (DataRow row in orderedRows)
            {
                if (!fieldValuesAreEqual(lastValues, row, FieldNames))
                {
                    newTable.ImportRow(row);

                    setLastValues(lastValues, row, FieldNames);
                }
            }
            return newTable;
        }

        private bool fieldValuesAreEqual(object[] lastValues, DataRow currentRow, string[] fieldNames)
        {
            bool areEqual = true;

            for (int i = 0; i < fieldNames.Length; i++)
            {
                if (lastValues[i] == null || !lastValues[i].Equals(currentRow[fieldNames[i]]))
                {
                    areEqual = false;
                    break;
                }
            }

            return areEqual;
        }

        private void setLastValues(object[] lastValues, DataRow sourceRow, string[] fieldNames)
        {
            for (int i = 0; i < fieldNames.Length; i++)
                lastValues[i] = sourceRow[fieldNames[i]];
        }
        #endregion

        #endregion

        /// <summary>
        /// 重载函数 
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="IsConvert">是否进行数据处理 true 处理</param>
        /// <param name="IsDistinguish"></param>
        /// <returns></returns>
        public List<T> FillModel(DataSet ds, bool IsConvert, bool IsDistinguish)
        {
            if (ds == null || ds.Tables[0] == null || ds.Tables[0].Rows.Count == 0)
            {
                return null;
            }
            else
            {
                return FillModel(ds.Tables[0], IsConvert, IsDistinguish);
            }
        }

        /// <summary>  
        /// 填充对象列表：用DataTable填充实体类
        /// </summary>  
        /// <param name="IsConvert">是否进行数据处理 true 处理</param>
        /// <param name="IsDistinguish">是否区分大小写 True 区分</param>
        /// <returns></returns>
        public List<T> FillModel(DataTable dt, bool IsConvert, bool IsDistinguish)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                return null;
            }
            List<T> modelList = new List<T>();
            foreach (DataRow dr in dt.Rows)
            {
                T model = new T();
                for (int i = 0; i < dr.Table.Columns.Count; i++)
                {
                    var columnValue = dr[i];
                    var columnInfo = dr.Table.Columns[i];
                    PropertyInfo propertyInfo = null;
                    if (IsDistinguish)
                        propertyInfo = model.GetType().GetProperty(columnInfo.ColumnName);
                    else
                        propertyInfo = model.GetType().GetProperty(columnInfo.ColumnName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                    if (propertyInfo != null && columnValue != DBNull.Value)
                    {
                        if (IsConvert)
                        {
                            if (columnInfo.DataType == typeof(decimal))
                            {
                                decimal d = Math.Ceiling(Convert.ToDecimal(columnValue) * 100) / 100;
                                SetPropValue(model, propertyInfo, Math.Round(d, 2));
                            }
                            else
                                SetPropValue(model, propertyInfo, columnValue);
                        }
                        else
                            SetPropValue(model, propertyInfo, columnValue);
                    }
                }

                modelList.Add(model);
            }
            return modelList;
        }

        /// <summary>  
        /// 填充对象：用DataRow填充实体类
        /// </summary>  
        /// <param name="dr">行数据</param>
        /// <param name="IsConvert">是否进行数据处理 true 处理</param>
        /// <param name="IsDistinguish">是否区分大小写 True 区分</param>
        /// <returns></returns>
        public T FillModel(DataRow dr, bool IsConvert, bool IsDistinguish)
        {
            if (dr == null)
            {
                return default(T);
            }

            T model = new T();

            for (int i = 0; i < dr.Table.Columns.Count; i++)
            {
                var columnValue = dr[i];
                var columnInfo = dr.Table.Columns[i];
                PropertyInfo propertyInfo = null;
                if (IsDistinguish)
                    propertyInfo = model.GetType().GetProperty(columnInfo.ColumnName);
                else
                    propertyInfo = model.GetType().GetProperty(columnInfo.ColumnName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (propertyInfo != null && dr[i] != DBNull.Value)
                {
                    if (IsConvert)
                    {
                        if (columnInfo.DataType == typeof(decimal))
                        {
                            decimal d = Math.Ceiling(Convert.ToDecimal(dr[i]) * 100) / 100;
                            SetPropValue(model, propertyInfo, Math.Round(d, 2));
                        }
                        else
                            SetPropValue(model, propertyInfo, columnValue);
                    }
                    else
                        SetPropValue(model, propertyInfo, columnValue);
                }
            }
            return model;
        }
    }

    /// <summary>
    /// 实体类转换类 
    /// </summary> 
    public class ModelHelpler
    {
        /// <summary>
        /// 将对象属性转换为key-value对
        /// </summary>
        /// <returns></returns>
        public static SortedDictionary<string, object> EntityToMap(object t)
        {
            SortedDictionary<string, object> map = new SortedDictionary<string, object>();
            Type type = t.GetType();

            PropertyInfo[] pi = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo p in pi)
            {
                MethodInfo mi = p.GetGetMethod();
                if (mi != null && mi.IsPublic)
                {
                    map.Add(p.Name, mi.Invoke(t, new object[] { }));
                }
            }
            return map;
        }

        /// <summary>
        /// 对象拷贝
        /// </summary>
        /// <typeparam name="Tsource">源</typeparam>
        /// <typeparam name="Ttarget">目标</typeparam>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void MapObject<Tsource, Ttarget>(Tsource source, Ttarget target) where Ttarget : new()
        {
            if (source == null) throw new ArgumentNullException("source can't be null");
            if (target == null) target = new Ttarget();
            PropertyInfo[] sourceProperties = typeof(Tsource).GetProperties();
            PropertyInfo[] targetProperties = typeof(Ttarget).GetProperties();

            foreach (var sourceProperty in sourceProperties)
            {
                foreach (var targetProperty in targetProperties)
                {
                    if (sourceProperty.Name == targetProperty.Name && sourceProperty.PropertyType == targetProperty.PropertyType && targetProperty.CanWrite)
                    {
                        targetProperty.SetValue(target, sourceProperty.GetValue(source, null), null);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 对象拷贝
        /// </summary>
        /// <typeparam name="Ttarget">目标</typeparam>
        /// <param name="source">源</param>
        public static Ttarget MapObject<Ttarget>(object source) where Ttarget : new()
        {
            if (source == null) throw new ArgumentNullException("source can't be null");

            Type targetType = typeof(Ttarget);
            Type sourceType = source.GetType();
            PropertyInfo[] sourceProperties = sourceType.GetProperties();
            Ttarget target = new Ttarget();

            foreach (var sourceProperty in sourceProperties)
            {
                PropertyInfo targetProperty = targetType.GetProperty(sourceProperty.Name);
                if (targetProperty.CanWrite)
                {
                    targetProperty.SetValue(target, sourceProperty.GetValue(source));
                }
            }

            return target;
        }

        /// <summary>
        /// 对象列表拷贝
        /// </summary>
        /// <typeparam name="Tsource"></typeparam>
        /// <typeparam name="Ttarget"></typeparam>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void MapObjects<Tsource, Ttarget>(List<Tsource> source, List<Ttarget> target)
        {
            if (source.IsNullOrEmpty()) throw new ArgumentNullException("source can't be null");
            if (target == null) target = new List<Ttarget>();

            PropertyInfo[] sourceProperties = typeof(Tsource).GetProperties();
            PropertyInfo[] targetProperties = typeof(Ttarget).GetProperties();

            foreach (var sourceItem in source)
            {
                var targetItem = Activator.CreateInstance<Ttarget>();
                foreach (var sourceProperty in sourceProperties)
                {
                    foreach (var targetProperty in targetProperties)
                    {
                        if (sourceProperty.Name == targetProperty.Name && sourceProperty.PropertyType == targetProperty.PropertyType && targetProperty.CanWrite)
                        {
                            var value = sourceProperty.GetValue(sourceItem, null);
                            targetProperty.SetValue(targetItem, value, null);
                            break;
                        }
                    }
                }
                target.Add(targetItem);
            }
        }

        /// <summary>
        /// 对象列表拷贝
        /// </summary>
        /// <typeparam name="Ttarget"></typeparam>
        /// <param name="source"></param>
        public static List<Ttarget> MapObjects<Ttarget>(List<object> source)
        {
            if (source.IsNullOrEmpty()) throw new ArgumentNullException("source can't be null");
            List<Ttarget> target = new List<Ttarget>();

            Type targetType = typeof(Ttarget);
            Type sourceType = source[0].GetType();
            PropertyInfo[] sourceProperties = sourceType.GetProperties();

            foreach (var sourceItem in source)
            {
                var targetItem = Activator.CreateInstance<Ttarget>();
                foreach (var property in sourceProperties)
                {
                    PropertyInfo targetProperty = targetType.GetProperty(property.Name);
                    if (targetProperty.CanWrite)
                    {
                        targetProperty.SetValue(targetItem, property.GetValue(sourceItem, null), null);
                    }
                }
                target.Add(targetItem);
            }

            return target;
        }
    }
}
