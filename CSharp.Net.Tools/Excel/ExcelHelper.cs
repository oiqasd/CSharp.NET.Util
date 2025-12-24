using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace CSharp.Net.Tools
{
    public class ExcelHelper
    {
        #region 导入
        /// <summary>
        /// 导入Excel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="headerIndex"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(string filePath, int headerIndex = 0) where T : new()
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found.", filePath);
            }

            try
            {
                using (var stream = File.OpenRead(filePath))
                {
                    return ToList<T>(stream, headerIndex);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 导入Excel
        /// </summary>
        /// <param name="file">导入文件</param>
        /// <returns>List<T></returns>
        public static List<T> ToList<T>(Stream stream, int headerIndex = 0) where T : new()
        {
            //string fileExt = Path.GetExtension(file.FileName).ToLower();
            //if (fileExt == ".xlsx")
            //{
            //    workbook = new XSSFWorkbook(ms);
            //}

            IWorkbook workbook = null;
            try
            {
                workbook = new XSSFWorkbook(stream);
            }
            catch
            {
                try
                {
                    workbook = new HSSFWorkbook(stream);
                }
                catch
                {
                    throw new FileLoadException("文件类型错误");
                }
            }
            ISheet sheet;
            try
            {
                List<T> list = new List<T> { };

                var columns = ExcelHelper.GetKeysByModelAttr<T>(false);
                if (columns.Count <= 0)
                {
                    throw new Exception("实体没有ExcelColumnNameAttribute标记字段");
                }

                sheet = workbook.GetSheetAt(0);
                IRow headrow = sheet?.GetRow(headerIndex);
                if (sheet == null || headrow == null)
                {
                    throw new FileLoadException("导入模板错误");
                }

                var propertys = typeof(T).GetProperties();
                string value = null;
                int num = headrow.LastCellNum;

                var excelColums = new Dictionary<string, int>();

                for (int i = 0; i < num; i++)
                {
                    string cell = headrow.GetCell(i)?.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(cell) && !excelColums.ContainsKey(cell))
                    {
                        excelColums.Add(cell, i);
                    }
                }
                bool exist = false;
                foreach (var c in columns)
                {
                    foreach (var e in excelColums)
                    {
                        if (e.Key == c.Key)
                        {
                            exist = true;
                            break;
                        }
                    }
                    if (exist)
                        break;
                }

                if (!exist) throw new FileLoadException("导入模板错误");

                for (int i = headerIndex + 1; i <= sheet.LastRowNum; i++)
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null)
                        continue;
                    var obj = new T();

                    exist = false;
                    foreach (var col in excelColums)
                    {
                        if (!columns.ContainsKey(col.Key)) continue;

                        value = row.GetCell(col.Value)?.ToString();
                        if (string.IsNullOrWhiteSpace(value)) continue;

                        var t = propertys.FirstOrDefault(x => x.Name == columns[col.Key]);
                        if (t == null) continue;

                        string strType = t.PropertyType.FullName;
                        if (strType == "System.String")
                        {
                            t.SetValue(obj, value?.Trim() ?? "");
                        }
                        else if (strType == "System.DateTime")
                        {
                            DateTime pdt = Convert.ToDateTime(value, CultureInfo.InvariantCulture);
                            t.SetValue(obj, pdt);
                        }
                        else if (strType == "System.Int32")
                        {
                            int pi32 = Convert.ToInt32(value);
                            t.SetValue(obj, pi32);
                        }
                        else if (strType == "System.Int64")
                        {
                            long pi64 = Convert.ToInt64(value);
                            t.SetValue(obj, pi64);
                        }
                        else if (strType == typeof(Decimal).FullName)
                        {
                            decimal de = Convert.ToDecimal(value);
                            t.SetValue(obj, de);
                        }
                        else if (strType == typeof(Boolean).FullName)
                        {
                            bool.TryParse(value, out bool result);
                            t.SetValue(obj, result);
                        }
                        else if (t.PropertyType.IsEnum)
                        {
                            var em = Enum.ToObject(t.PropertyType, int.Parse(value));
                            t.SetValue(obj, em);
                        }
                        else
                        {
                            t.SetValue(obj, null);
                        }
                        exist = true;
                    }
                    if (exist)
                        list.Add(obj);
                }
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                sheet = null;
                workbook = null;
            }
        }

        /// <summary>
        /// excel转datatable
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="headerIndex">标题行</param>
        /// <param name="sheetIndex">sheet</param>
        /// <returns></returns>
        public static DataTable ToDataTable(Stream stream, int headerIndex = 0, int sheetIndex = 0)
        {
            if (stream == null || stream.Length <= 0)
                return null;

            IWorkbook workbook = null;
            try
            {
                workbook = new XSSFWorkbook(stream);
            }
            catch
            {
                try
                {
                    workbook = new HSSFWorkbook(stream);
                }
                catch
                {
                    throw new FileLoadException("文件类型错误");
                }
            }

            ISheet sheet = workbook.GetSheetAt(sheetIndex);
            IRow headrow = sheet.GetRow(headerIndex);

            DataTable dt = new DataTable(sheet.SheetName);
            int num = headrow.LastCellNum;

            for (int i = 0; i < num; i++)
            {
                string cell = headrow.GetCell(i)?.ToString().Trim();
                if (string.IsNullOrWhiteSpace(cell))
                {
                    cell = "cell_" + i.ToString();
                }
                if (dt.Columns.Contains(cell))
                {
                    cell = cell + "_" + i.ToString();
                }

                dt.Columns.Add(cell);
            }

            for (int i = headerIndex + 1; i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row == null)
                    continue;

                var dr = dt.NewRow();

                for (int j = 0; j < num; j++)
                {
                    dr[j] = row.GetCell(j)?.ToString();
                }
                dt.Rows.Add(dr);
            }

            return dt;
        }
        #endregion

        #region 导出

        public static byte[] ExportExcel<T>(List<T> list)
        {
            StringBuilder sb = new StringBuilder();
            var properties = typeof(T).GetProperties();
            for (int i = 0; i < properties.Count(); i++)
            {
                sb.Append(properties[i].Name);
                if (i == properties.Count() - 1) continue;
                sb.Append(",");
            }
            foreach (var r in list)
            {
                sb.AppendLine();

                for (int i = 0; i < properties.Count(); i++)
                {
                    sb.Append(properties[i].GetValue(r)?.ToString());
                    if (i == properties.Count() - 1) continue;
                    sb.Append(",");
                }
            }
            return Encoding.Default.GetBytes(sb.ToString());
        }

        /// <summary>
        /// 使用模板导出excel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filepath"></param>
        /// <param name="list"></param>
        /// <param name="headerIndex"></param>
        /// <returns></returns>
        public static byte[] ExportExcel<T>(string filepath, IEnumerable<T> list, int headerIndex = 0)
        {
            byte[] data;
            MemoryStream ms = null;
            IWorkbook workbook = null;
            ISheet sheet;

            try
            {
                string fileExt = Path.GetExtension(filepath).ToLower();
                FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                if (list == null || !list.Any())
                {
                    data = new byte[fs.Length];
                    fs.Read(data, 0, data.Length);
                    return data;
                }
                if (fileExt == ".xlsx")
                {
                    workbook = new XSSFWorkbook(fs);
                }
                else
                {
                    workbook = new HSSFWorkbook(fs);
                }

                sheet = workbook.GetSheetAt(0);
                int maxclomun = sheet.GetRow(headerIndex).LastCellNum;

                if (maxclomun <= 0)
                {
                    throw new Exception("Excel模板没有字段");
                }

                var properties = typeof(T).GetProperties();

                var columns = ExcelHelper.GetKeysByModelAttr<T>();
                if (columns.Count <= 0)
                {
                    throw new Exception("实体没有ExcelColumnNameAttribute标记字段");
                }

                IRow headrow = sheet.GetRow(headerIndex);
                var excelColums = new Dictionary<string, int>();

                for (int i = 0; i < maxclomun; i++)
                {
                    string cell = headrow.GetCell(i)?.ToString().Trim();
                    if (string.IsNullOrWhiteSpace(cell))
                    {
                        cell = "cell_" + i.ToString();
                    }
                    if (excelColums.ContainsKey(cell))
                    {
                        cell = cell + "_" + i.ToString();
                    }
                    excelColums.Add(cell, i);
                }

                int rowIndex = headerIndex + 1;
                foreach (var i in list)
                {
                    IRow dataRow = sheet.CreateRow(rowIndex);

                    foreach (PropertyInfo p in properties)
                    {
                        if (!columns.ContainsKey(p.Name))
                            continue;

                        string cname = columns[p.Name];
                        if (!excelColums.ContainsKey(cname))
                            continue;

                        CellType cellType = CellType.String;

                        if (p.PropertyType == typeof(Int32) || p.PropertyType == typeof(Int64) || p.PropertyType == typeof(Decimal) || p.PropertyType == typeof(Double))
                        {
                            cellType = CellType.Numeric;
                        }
                        dataRow.CreateCell(excelColums[cname], cellType).SetCellValue(p.GetValue(i)?.ToString());
                    }

                    rowIndex++;
                }

                ms = new MemoryStream();
                workbook.Write(ms);
                ms.Flush();
                //data = ms.GetBuffer();
                data = ms.ToArray();
                return data;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (ms != null)
                    ms.Close();
                sheet = null;
                workbook = null;
            }

            return null;
        }

        /// <summary>
        /// 无模板导出
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="SheetName"></param>
        /// <param name="list"></param>
        /// <param name="fieldNames">("标题名称","<T>实体字段")</param>
        /// <returns></returns>
        public static byte[] Export<T>(string SheetName, List<T> list, Dictionary<string, string> fieldNames)
        {
            HSSFWorkbook wb = new HSSFWorkbook();
            HSSFSheet sheet;
            try
            {
                byte[] data;
                sheet = (HSSFSheet)wb.CreateSheet(SheetName); //创建工作表
                sheet.CreateFreezePane(0, 1); //冻结列头行
                HSSFRow row_Title = (HSSFRow)sheet.CreateRow(0); //创建列头行
                row_Title.HeightInPoints = 30.5F; //设置列头行高
                HSSFCellStyle cs_Title = (HSSFCellStyle)wb.CreateCellStyle(); //创建列头样式
                cs_Title.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center; //水平居中
                cs_Title.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center; //垂直居中
                HSSFFont cs_Title_Font = (HSSFFont)wb.CreateFont(); //创建字体
                cs_Title_Font.IsBold = true; //字体加粗
                cs_Title_Font.FontHeightInPoints = 14; //字体大小
                cs_Title.SetFont(cs_Title_Font); //将字体绑定到样式
                #region 生成列头
                int ii = 0;
                foreach (string key in fieldNames.Keys)
                {
                    HSSFCell cell_Title = (HSSFCell)row_Title.CreateCell(ii); //创建单元格
                    cell_Title.CellStyle = cs_Title; //将样式绑定到单元格
                    cell_Title.SetCellValue(key);
                    sheet.SetColumnWidth(ii, 25 * 256);//设置列宽
                    ii++;
                }

                #endregion
                //获取 实体类 类型对象
                Type t = typeof(T); // model.GetType();
                //获取 实体类 所有的 公有属性
                List<PropertyInfo> proInfos = t.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList();
                //创建 实体属性 字典集合
                Dictionary<string, PropertyInfo> dictPros = new Dictionary<string, PropertyInfo>();
                //将 实体属性 中要修改的属性名 添加到 字典集合中 键：属性名  值：属性对象
                proInfos.ForEach(p =>
                {
                    if (fieldNames.Values.Contains(p.Name))
                    {
                        dictPros.Add(p.Name, p);
                    }
                });

                HSSFCellStyle cs_Content = (HSSFCellStyle)wb.CreateCellStyle(); //创建列头样式
                cs_Content.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center; //水平居中
                cs_Content.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center; //垂直居中

                if (list != null)
                    for (int i = 0; i < list.Count; i++)
                    {

                        HSSFRow row_Content = (HSSFRow)sheet.CreateRow(i + 1); //创建行
                        row_Content.HeightInPoints = 20;
                        int jj = 0;
                        foreach (string proName in fieldNames.Values)
                        {
                            if (dictPros.ContainsKey(proName))
                            {
                                HSSFCell cell_Conent = (HSSFCell)row_Content.CreateCell(jj); //创建单元格
                                cell_Conent.CellStyle = cs_Content;

                                //如果存在，则取出要属性对象
                                PropertyInfo proInfo = dictPros[proName];
                                //获取对应属性的值
                                object value = proInfo.GetValue(list[i], null); //object newValue = model.uName;
                                string cell_value = value == null ? "" : value.ToString();
                                cell_Conent.SetCellValue(cell_value);
                                jj++;
                            }
                        }
                    }

                using (MemoryStream ms = new MemoryStream())
                {
                    wb.Write(ms);
                    ms.Flush();
                    data = ms.ToArray();
                    return data;
                }
            }
            catch
            {
                return null;
            }
            finally
            {
                sheet = null;
                wb = null;
            }
        }

        /// <summary>
        /// 获取实体导出字段
        /// 使用ExcelColumnNameAttribute特性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="forImport">default:true,true:字段名做key,特性名做value;false相反.字段对应实体字段,特性名对应excel文件字段.</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetKeysByModelAttr<T>(bool forImport = true)
        {
            List<PropertyInfo> proInfos = typeof(T).GetProperties().ToList();
            Dictionary<string, string> keys = new Dictionary<string, string>();

            var newp = proInfos.Where(x => x.CustomAttributes.Any(t => t.AttributeType.Name == nameof(ExcelColumnNameAttribute))).ToList();
            var sortp = newp.OrderBy(x => ((ExcelColumnNameAttribute)x.GetCustomAttributes(typeof(ExcelColumnNameAttribute), true).FirstOrDefault()).Sort).ToList();

            foreach (var t in sortp)
            {
                var colval = ((ExcelColumnNameAttribute)t.GetCustomAttributes(typeof(ExcelColumnNameAttribute), true).FirstOrDefault()).ColumnName;
                var colname = t.Name;

                if (forImport)
                {
                    if (keys.ContainsKey(colname)) continue;
                    keys.Add(colname, colval);
                }
                else
                {
                    if (keys.ContainsKey(colval)) continue;
                    keys.Add(colval, colname);
                }
            }

            return keys;
        }

        #endregion
    }
}

