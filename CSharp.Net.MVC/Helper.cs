using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Text;
using System.Web;

namespace CSharp.Net.Mvc
{
    public static class Helper
    {
        /// <summary>
        /// 将图片转换为bytes[]
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        public static string ConversionBase(HttpRequest Request, int count)
        {
            if (Request.Form.Files.Count == 0)
                return null;
            if (Request.Form.Files[count].Length == 0)
                return null;

            IFormFile file = Request.Form.Files[count];
            System.IO.Stream stream = file.OpenReadStream();
            long FileLen = file.Length;
            byte[] bytes = new byte[FileLen];
            stream.Read(bytes, 0, Convert.ToInt32(FileLen));
            return Convert.ToBase64String(bytes);
        }

        #region 获得键值对集合的值

        public static string GetParamValue(NameValueCollection argNameValue, string pname)
        {
            if (argNameValue[pname] != null)
                return HttpUtility.UrlDecode(argNameValue[pname].ToString());
            return "";
        }

        public static string GetParamValue(NameValueCollection argNameValue, string pname, Encoding encode)
        {
            if (argNameValue[pname] != null)
                return HttpUtility.UrlDecode(argNameValue[pname].ToString(), encode);
            return "";
        }

        #endregion

        #region 数据类型转换

        public static T GetChangeTypeValue<T>(Object param, T defaultValue)
        {
            try
            {
                return (T)Convert.ChangeType(param, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        public static T GetChangeTypeValue<T>(Object param)
        {
            try
            {
                return (T)Convert.ChangeType(param, typeof(T));
            }
            catch
            {
                return default(T);
            }
        }

        public static T GetRequest<T>(string key, T defaultValue)
        {
            string value = RequestHelper.GetPostOrRequestValue(key, MethodType.All);
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }
            else
            {
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
        }

        #endregion

        #region 数据递归
        /// <summary>
        /// 数据递归排序(应用于checkbox)
        /// </summary>
        public static DataTable GetCheckBoxItems(DataTable dt, DataTable dtResult, string eleAttrName, string name, string id, string strParentID, string addStr)
        {
            string strFilter = null;
            if (strParentID != null && strParentID.Length > 0)
                strFilter = "ParentID = {0}";
            else
                strFilter = "ParentID is null";
            strFilter = string.Format(strFilter, strParentID);

            //开始递归排序
            DataRow[] drs = dt.Select(strFilter);
            foreach (DataRow dr in drs)
            {
                DataRow drNew = dtResult.NewRow();
                for (int i = 0; i < dr.Table.Columns.Count; i++)
                {
                    drNew[i] = dr[i];
                }
                drNew[name] = addStr + "<input type=\"checkbox\" name=\"" + eleAttrName + "\" value=\"" + drNew[id] + "\" />" + drNew[name];
                dtResult.Rows.Add(drNew);
                GetCheckBoxItems(dt, dtResult, eleAttrName, name, id, dr[id].ToString(), addStr + "\u3000");
            }

            return dtResult;
        }

        /// <summary>
        /// 数据递归排序(应用于droplist)
        /// </summary>
        public static DataTable GetDropItems(DataTable dt, DataTable dtResult, string name, string id, string strParentID, string addStr)
        {
            string strFilter = null;
            if (strParentID != null && strParentID.Length > 0)
                strFilter = "ParentID = {0}";
            else
                strFilter = "ParentID is null";
            strFilter = string.Format(strFilter, strParentID);

            //开始递归排序
            DataRow[] drs = dt.Select(strFilter);
            foreach (DataRow dr in drs)
            {
                DataRow drNew = dtResult.NewRow();
                for (int i = 0; i < dr.Table.Columns.Count; i++)
                {
                    drNew[i] = dr[i];
                }
                drNew[name] = addStr + drNew[name];
                dtResult.Rows.Add(drNew);
                GetDropItems(dt, dtResult, name, id, dr[id].ToString(), addStr + "\u3000");
            }

            return dtResult;
        }
        #endregion
    }
}
