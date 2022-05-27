using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using System.Text.Json;
using ITWebService.Core;
using ITWebService.Core.Config;
using ITWebService.Models;
using ITWebService.Core.DutyInfos;

namespace ITWebService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DutyInfoController : ControllerBase
    {
        private IWebHostEnvironment _he;
        public DutyInfoController(IWebHostEnvironment he)
        {
            _he = he;
        }
        [Route("GetInfo")]
        [HttpPost]
        public PersonOnDutyInfoModel Getinfo(PersonOnDutyInfoModel model)
        {
            DutyInfoComputeHander computeHander = new(ref model, model.Location.Length);
            return model;

        }
        [HttpGet("GetPersonInfo")]
        public IActionResult GetPersonInfo([FromQuery]string location)
        {
            var result = new List<PersonInfo>();
            DutyPeopleHander peopleHander = new Core.DutyInfos.DutyPeopleHander(location,ref result);
            return Ok(result);
        }
        /// <summary>
        /// 检查服务端是否存在对应的文件
        /// </summary>
        /// <param name="pathlist"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool Checkfile(List<string> pathlist, ref string message)
        {
            bool temp = true;
            foreach (var path in pathlist)
            {
                if (!System.IO.File.Exists(path))
                {
                    message += $"服务端还没有：{path}文件，请上传！\n";
                    temp = false;
                }
            }
            return temp;
        }
        [Route("GetSiteInfo")]
        [HttpGet]
        public string[] GetSiteInfo()
        {
            var str = System.IO.File.ReadAllText(Path.Combine(ConfigCore.WebRootPath, "/etc/ITWebService/DutyInfo/Sites.json"));
            var siteinfo = JsonSerializer.Deserialize<string[]>(str);
            return siteinfo ?? new string[] { };
        }
    }
}
