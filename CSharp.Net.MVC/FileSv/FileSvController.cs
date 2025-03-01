using CSharp.Net.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.Net.Mvc.FileSv
{
    [Route("fileSv/[action]")]
    public class FileSvController : BaseController
    {
        public FileSvController()
        {

        }

        [HttpGet]
        public Response<string> Login()
        {
            return Success("ok");
        }

        [HttpPost]
        public async Task<Response> Upload(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return Fail(ReturnCode.PamramerError, "No file uploaded.");

            var uploadedFiles = new List<string>();
            foreach (var file in files)
            {
                var filePath = Path.Combine(AppDomainHelper.GetRunRoot, file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            return Success("ok");
        }

        [HttpGet]
        public IActionResult Download(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return BadRequest("File name must be provided.");

            var filePath = Path.Combine(AppDomainHelper.GetRunRoot, fileName);
            if (!FileHelper.IsFileExists(filePath))
                return NotFound("File not found.");

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", fileName);
        }


        [HttpGet]
        public async Task<Response<string>> FileList()
        {
            StringBuilder sb=new StringBuilder();
            foreach (var fn in Directory.GetFiles(AppDomainHelper.GetRunRoot))
            {
                sb.AppendLine(fn);
            } 
            return Success(sb.ToString());
        }
    }
}

