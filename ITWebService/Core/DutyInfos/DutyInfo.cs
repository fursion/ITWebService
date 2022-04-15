using System;
using System.Text;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using ITWebService.Core.Config;

namespace ITWebService.Core
{
    public enum Shift
    {
        Day_shift,
        Middle_shift,
        Primary_Day_shift,
        Primary_Night_shift,
    }
    public static class DutyInfo
    {
        public static Dictionary<string, string> link_dict;
        public static Dictionary<string, string> Templatedict;
        public static Dictionary<string, Dutyinfos> Dutyinfo_dict;
        public static void Init(string folderpath)
        {
            IO.CheckPath(folderpath, true);
            Read_Dutyinfo_Dict(folderpath);
            Read_Linkinfo_Dict(Path.Combine(ConfigCore.WebRootPath, ConfigCore.GetConfigItem<DutyConfig>("DutyConfig").SavePath, ConfigCore.GetConfigItem<DutyConfig>("DutyConfig").FolderPath));
            Read_Templatedict(folderpath);
        }
        public static void Read_Linkinfo_Dict(string rootpath)
        {
            try
            {
                var linkinfo = File.ReadAllText(Path.Combine(rootpath, "linklist.json"));
                link_dict = JsonSerializer.Deserialize<Dictionary<string, string>>(linkinfo);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }
        public static void Read_Dutyinfo_Dict(string rootpath)
        {
            try
            {
                var Dutyinfo = File.ReadAllText(Path.Combine(rootpath, "DutyInfo.json"));
                Dutyinfo_dict = JsonSerializer.Deserialize<Dictionary<string, Dutyinfos>>(Dutyinfo);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

        }
        public static void Read_Templatedict(string rootpath)
        {
            try
            {
                var Templateinfo = File.ReadAllText(Path.Combine(rootpath, "Template.json"));
                Templatedict = JsonSerializer.Deserialize<Dictionary<string, string>>(@Templateinfo);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }
    }
    [Serializable]
    public class Dutyinfos
    {
        public string icon { get; set; }
        public string Place { get; set; }
        public string Timeslot { get; set; }
    }
}