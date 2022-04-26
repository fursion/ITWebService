using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using ITWebService.Core;
using ITWebService.Core.Config;
namespace ITWebService.Core.DutyInfos
{
    public class DutyInfoService : WebService<DutyInfoService>, IWebService
    {
        public Dictionary<string, string> ContactsLinksDict;
        public DutyInfoService()
        {
            ConfigCore.AddConfig<DutyConfig>();
            Refresh();
        }
        public void Refresh()
        {
            try
            {
                var path = Path.Combine(ConfigCore.WebRootPath,ConfigCore.GetConfigItem<DutyConfig>().FolderPath, ConfigCore.GetConfigItem<DutyConfig>().ContactLinkPath);
                //Console.WriteLine(path);
                ContactsLinksDict = JsonSerializer.Deserialize<Dictionary<string, string>>(IO.ReadAllText(path));
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
        }
        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }

        void IWebService.ReStartService()
        {
            throw new NotImplementedException();
        }

        void IWebService.StartService()
        {
            throw new NotImplementedException();
        }

        void IWebService.StopService()
        {
            throw new NotImplementedException();
        }
    }
}

