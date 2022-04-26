using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using ITWebService.Core;
using ITWebService.Core.Config;

namespace ITWebService.Core
{
    public static class IO
    {
        public static void ConnectSql()
        {

        }

        public static void WriteToSQL()
        {


        }
        public static string ReadAllText(string path)
        {
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }
            return "文件不存在";
        }
        public static string[] ReadAllLines(string path)
        {
            if (File.Exists(path))
            {
                var lines = File.ReadAllLines(path);
                return lines;
            }
            return new string[] { "文件不存在" };
        }
        public static void PossingFile(HttpContext httpContext)
        {
            var f = httpContext.Request.Form.Files.FirstOrDefault();
            Console.WriteLine($"IO :{f.FileName}");
        }
        /// <summary>
        /// 检查目录是否存在，不存在则根据Create属性创建目录
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="Create">创建属性，控制是否创建目录</param>
        public static bool CheckPath(string path, bool Create = false)
        {
            if (Create)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);

                }
                return Directory.Exists(path);
            }
            else
                return Directory.Exists(path);
        }
        /// <summary>
        /// 保存上传到服务器的文件
        /// </summary>
        /// <param name="files"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<IActionResult> SaveUpLoadfile(List<IFormFile> files, string path = null)
        {
            long size = files.Sum(f => f.Length);
            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    if (null == path)
                        path = Path.Combine(ConfigCore.WebRootPath, ConfigCore.TempFilePath, formFile.FileName);
                    Console.WriteLine(path);
                    IO.CheckPath(path, true);
                    using (var stream = System.IO.File.Create(path))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }
            OkObjectResult ok = new(new { count = files.Count, size });
            return ok;
        }
        public static string strtopath(string pathstr){
            System.Console.WriteLine(Path.Combine(pathstr));
            return "/";
        }
    }
}
