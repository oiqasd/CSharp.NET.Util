using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CSharp.Net.Util.Interface
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFileUpload
    {
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="fileStream"></param> 
        /// <returns></returns>
        UploadFileOutDto UploadFile(FileStream fileStream);
        /// <summary>
        /// 获取文件服务器路径
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="deleteLocal">是否删除本地文件</param> 
        /// <returns></returns>
        UploadFileOutDto UploadFileByPath(string filepath, bool deleteLocal = false);
        
        /// <summary>
        /// 获取文件地址
        /// </summary>
        /// <param name="fileKey"></param>
        /// <returns></returns>
        string GetFilePath(string fileKey);
    }
}
