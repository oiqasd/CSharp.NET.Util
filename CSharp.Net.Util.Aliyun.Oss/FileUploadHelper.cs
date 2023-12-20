using Aliyun.OSS;
using Aliyun.OSS.Common;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CSharp.Net.Util.Aliyun.Oss
{
    public class FileUploadHelper : IFileUpload
    {

        FileUploadAliyunModel _model;
        public FileUploadHelper(FileUploadAliyunModel model)
        {
            _model = model;
        }


        /// <summary>
        /// 上传文件到Oss
        /// </summary>
        /// <param name="fileStream"></param> 
        /// <returns>文件路径</returns>
        public UploadFileOutDto UploadFile(FileStream fileStream)
        {
            return UpOss(fileStream);
        }

        /// <summary>
        /// 获取文件服务器路径
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="deleteLocal"></param>
        /// <returns></returns>
        public UploadFileOutDto UploadFileByPath(string filepath, bool deleteLocal = false)
        {

            UploadFileOutDto dto = null;

            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException("文件不存在!");
            }

            using (FileStream fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                dto = UpOss(fileStream);

                //删除本地文件
                if (deleteLocal)
                {
                    Task.Factory.StartNew(() =>
                    {
                        if (!string.IsNullOrEmpty(filepath))
                        {
                            if (File.Exists(filepath)) File.Delete(filepath);
                        }
                    });
                }
            }
            return dto;
        }

        /// <summary>
        /// 获取文件oss地址
        /// </summary>
        /// <param name="fileKey"></param>
        /// <returns></returns>
        public string GetFilePath(string fileKey)
        {
            OssClient ossClient = OssManager.GetInstance(_model.AccessId, _model.AccessKey, _model.EndPoint);
            var uri = ossClient.GeneratePresignedUri(_model.BucketName, fileKey);
            return $"{uri.Scheme}://{uri.Host}/{fileKey}";
        }

        private UploadFileOutDto UpOss(FileStream stream)
        {
            try
            {
                UploadFileOutDto dto = new UploadFileOutDto();
                if (stream == null || stream.Length <= 0)
                {
                    throw new Exception("文件不能为空!");
                }

                dto.FileName = GenerateFileName(stream.Name);

                //获取该文件的输入流
                using (var fs = stream)
                {
                    OssClient ossClient = OssManager.GetInstance(_model.AccessId, _model.AccessKey, _model.EndPoint);
                    ossClient.SetBucketAcl(_model.BucketName, CannedAccessControlList.PublicRead);
                    //将该文件流保存到OSS中
                    var ret = ossClient.PutObject(_model.BucketName, Path.Combine(_model.Path, dto.FileName), fs);
                    var uri = ossClient.GeneratePresignedUri(_model.BucketName, dto.FileName);
                    dto.FilePath = $"{uri.Scheme}://{uri.Host}/{dto.FileName}";
                }

                return dto;

            }
            catch (OssException oe)
            {
                throw new Exception($"调用OSS上传时，访问服务器端时出错。错误码Code:{oe.ErrorCode},错误信息Message:{oe.Message}");
            }
        }

        private string GenerateFileName(string filename)
        {
            var ext = Path.GetExtension(filename);
            return $"{Utils.GuidToLongID()}{ext}";
        }

        private Stream FileToStream(string fileName)
        {

            // 打开文件
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {

                // 读取文件的 byte[]
                byte[] bytes = new byte[fileStream.Length];

                fileStream.Read(bytes, 0, bytes.Length);

                fileStream.Dispose();
                fileStream.Close();

                // 把 byte[] 转换成 Stream
                using Stream stream = new MemoryStream(bytes);
                return stream;
            }

        }


    }
}
