﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp.Net.Standard.Util
{
    /// <summary>
    /// 文件上传返回Dto
    /// </summary>
    public class UploadFileOutDto
    {
        /// <summary>
        /// 文件标识
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; }

    }
}
