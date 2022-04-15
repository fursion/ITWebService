using System;
namespace ITWebService.Core.Config
{
    public class DutyConfig : IConfig
    {
        public string SavePath { get; set; }
        public string ContactLinkPath { get; set; }
        public string TemplatePath { get; set; }
        public string TempHeader { get; set; }
        public string TempTail { get; set; }
        public string SheetName { get; set; }
        public string FolderPath { get; set; }
    }
}

