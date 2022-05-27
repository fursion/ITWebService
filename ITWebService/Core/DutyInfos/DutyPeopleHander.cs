// fursionDutyPeopleHander.cs
using System;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Data;
using System.Collections.Generic;
using ITWebService.Core;
using ITWebService.Core.Tools;
using ITWebService.Models;
using System.IO;
using ITWebService.Core.Config;
namespace ITWebService.Core.DutyInfos
{
    public struct PersonInfo
    {
        public string Icon { get; set; }
        public bool IsTrue { get; set; }
        public string Name { get; set; }
        public string Duty { get; set; }
        public string Location { get; set; }
        public string DutyTime { get; set; }
        public string Link { get; set; }
        public PersonInfo(string icon, string name, string duty, string location, string dutytime, string link)
        {
            this.IsTrue = true;
            this.Icon = icon;
            this.Name = name.Nameformat(); this.Duty = duty; this.Location = location; this.DutyTime = dutytime; this.Link = link;
        }
    }
    public class DutyPeopleHander : IDisposable
    {
        
        private string? PersonInfoTemplate;
        private DutyRule DutyRule { get; set; }
        private Dictionary<string, Dutyinfos>? DutyinfosDict { get; set; }
        public DutyPeopleHander(string location, ref List<PersonInfo> infos)
        {
            var rulefilepath = Path.Combine(ConfigCore.WebRootPath, ConfigCore.GetConfigItem<DutyConfig>().FolderPath, location, "Rules.json");//生成班表文件路径
            var Dutyinfosfilepath = Path.Combine(ConfigCore.WebRootPath, ConfigCore.GetConfigItem<DutyConfig>().FolderPath, location, "DutyInfo.json");
            PersonInfoTemplate = IO.ReadAllText(Path.Combine(ConfigCore.WebRootPath, ConfigCore.GetConfigItem<DutyConfig>().FolderPath, location, "TempInfo.txt"));
            DutyRule = getDutyRule(rulefilepath);
            DutyinfosDict = getDutyinfosDict(Dutyinfosfilepath);
            infos = ComputeDutyInfos(location);
        }
        /// <summary>
        /// 获取数据表
        /// </summary>
        public List<DataTable> getDataTable(string path)
        {
            var tables = Tools.ExcelTools.ExcelToDatatable(path);
            return tables;
        }
        /// <summary>
        /// 计算位置
        /// </summary>
        public PersonInfo ComputeLocation(DataTable table, DutyRule rule, DateTime time, int PersonIndex)
        {
            int NowMouthDays = Thread.CurrentThread.CurrentUICulture.Calendar.GetDaysInMonth(time.Year, time.Month);
            int LastRestDay;
            string todayDuty = table.Rows[PersonIndex][table.Columns[time.Day]].ToString();
            if (DoMaskItem(todayDuty) | todayDuty == "")
                return new PersonInfo();
            if (rule == null || rule.LocationInOnly)
            {
                if (IsDispute(todayDuty, DutyRule))
                {
                    //计算休息的日期
                    for (int i = time.Day; i >= 1; i--)
                    {
                        if (i == 1)
                        {
                            for (int i2 = time.Day; i2 <= NowMouthDays; i2++)
                            {
                                var tep1 = table.Rows[PersonIndex][table.Columns[i2]];
                                if (tep1.ToString().Contains("休"))
                                {
                                    LastRestDay = i2;
                                    var index = rule.Cycle - (LastRestDay - time.Day) % rule.Cycle;
                                    if (rule.Dispute[todayDuty].ContainsKey(index))
                                        return new PersonInfo($"{DutyinfosDict[DutyNameformat(todayDuty)].icon}", $"{table.Rows[PersonIndex][table.Columns[0]]}", $"{DutyNameformat($"{table.Rows[PersonIndex][table.Columns[time.Day]]}")}", $"{rule.Dispute[todayDuty][index]}", $"{DutyinfosDict[DutyNameformat(todayDuty)].Timeslot}", ComputePersonLinks($"{table.Rows[PersonIndex][table.Columns[0]]}"));
                                    else
                                        return new PersonInfo($"{DutyinfosDict[DutyNameformat(todayDuty)].icon}", $"{table.Rows[PersonIndex][table.Columns[0]]}", $"{DutyNameformat($"{table.Rows[PersonIndex][table.Columns[time.Day]]}")}", $"{DutyinfosDict[DutyNameformat(todayDuty)].Place}", $"{DutyinfosDict[DutyNameformat(todayDuty)].Timeslot}", ComputePersonLinks($"{table.Rows[PersonIndex][table.Columns[0]]}"));
                                }
                            }
                        }
                        else
                        {
                            var tep = table.Rows[PersonIndex][table.Columns[i]];
                            if (tep.ToString().Contains("休"))
                            {
                                LastRestDay = i;
                                var index = (time.Day - LastRestDay) % rule.Cycle;
                                if (rule.Dispute[todayDuty].ContainsKey(index))
                                    return new PersonInfo($"{DutyinfosDict[DutyNameformat(todayDuty)].icon}", $"{table.Rows[PersonIndex][table.Columns[0]]}", $"{DutyNameformat($"{table.Rows[PersonIndex][table.Columns[time.Day]]}")}", $"{rule.Dispute[todayDuty][index]}", $"{DutyinfosDict[DutyNameformat(todayDuty)].Timeslot}", ComputePersonLinks($"{table.Rows[PersonIndex][table.Columns[0]]}"));
                                else
                                    return new PersonInfo($"{DutyinfosDict[DutyNameformat(todayDuty)].icon}", $"{table.Rows[PersonIndex][table.Columns[0]]}", $"{DutyNameformat($"{table.Rows[PersonIndex][table.Columns[time.Day]]}")}", $"{DutyinfosDict[DutyNameformat(todayDuty)].Place}", $"{DutyinfosDict[DutyNameformat(todayDuty)].Timeslot}", ComputePersonLinks($"{table.Rows[PersonIndex][table.Columns[0]]}"));
                            }
                        }
                    }
                    return new PersonInfo($"{DutyinfosDict[DutyNameformat(todayDuty)].icon}", $"{table.Rows[PersonIndex][table.Columns[0]]}", $"{DutyNameformat($"{table.Rows[PersonIndex][table.Columns[time.Day]]}")}", $"{DutyinfosDict[DutyNameformat(todayDuty)].Place}", $"{DutyinfosDict[DutyNameformat(todayDuty)].Timeslot}", ComputePersonLinks($"{table.Rows[PersonIndex][table.Columns[0]]}"));
                }
                else
                    return new PersonInfo($"{DutyinfosDict[DutyNameformat(todayDuty)].icon}", $"{table.Rows[PersonIndex][table.Columns[0]]}", $"{DutyNameformat($"{table.Rows[PersonIndex][table.Columns[time.Day]]}")}", $"{DutyinfosDict[DutyNameformat(todayDuty)].Place}", $"{DutyinfosDict[DutyNameformat(todayDuty)].Timeslot}", ComputePersonLinks($"{table.Rows[PersonIndex][table.Columns[0]]}"));
            }
            else
                return new PersonInfo($"{DutyinfosDict[DutyNameformat(todayDuty)].icon}", $"{table.Rows[PersonIndex][table.Columns[0]]}", $"{DutyNameformat($"{table.Rows[PersonIndex][table.Columns[time.Day]]}")}", $"{DutyinfosDict[DutyNameformat(todayDuty)].Place}", $"{DutyinfosDict[DutyNameformat(todayDuty)].Timeslot}", ComputePersonLinks($"{table.Rows[PersonIndex][table.Columns[0]]}"));
        }
        /// <summary>
        /// 是否是争议项
        /// </summary>
        /// <returns></returns>
        private bool IsDispute(string input, DutyRule rule)
        {
            if (rule.Dispute.ContainsKey(input))
                return true;
            return false;
        }
        /// <summary>
        /// 执行遮罩剔除
        /// </summary>
        /// <param name="Item"></param>
        /// <returns></returns>
        private bool DoMaskItem(string Item)
        {
            bool result = false;
            for (int itemcount = 0; itemcount < DutyRule.MaskItem.Length; itemcount++)
            {
                result |= Item.Contains(DutyRule.MaskItem[itemcount]);
            }
            return result;
        }
        /// <summary>
        /// 计算工作时间
        /// </summary>
        private string TemplateCompose(string Temp, PersonInfo info)
        {
            return string.Format(Temp, info.Icon, info.Duty, info.Location, info.DutyTime, info.Name, info.Link);
        }
        private string ComputePersonLinks(string PersonName)
        {
            if (DutyInfoService.GetService().ContactsLinksDict.ContainsKey(PersonName))
            {
                var link = DutyInfoService.GetService().ContactsLinksDict[PersonName];
                return link;
            }
            return "not found person's link";
        }
        /// <summary>
        /// 计算在班人员信息
        /// </summary>
        private List<PersonInfo> ComputeDutyInfos(string location)
        {
            var dutyfilepath = Path.Combine(ConfigCore.WebRootPath, ConfigCore.GetConfigItem<DutyConfig>().FolderPath, location, $"{location}duty.xlsx");//生成班表文件路径
            var tables = getDataTable(dutyfilepath);
            return AnalysisDutyDatatable(TimeEffective(DateTime.Now), tables[0]);

        }
        /// <summary>
        /// 解析班表
        /// </summary>
        /// <param name="time"></param>
        /// <param name="table"></param>
        private List<PersonInfo> AnalysisDutyDatatable(DateTime time, DataTable table)
        {
            int day = time.Day;
            int NowMouthDays = Thread.CurrentThread.CurrentUICulture.Calendar.GetDaysInMonth(time.Year, time.Month);
            int ContactsIndex = 2;
            string TimeLocation = table.Rows[0][table.Columns[0]].ToString() ?? "";
            if (TimeLocation.Contains(time.Year.ToString()) && TimeLocation.Contains(time.Month.ToString()))//判断表是否为当前月
            {
                var persons = new List<PersonInfo>();
                for (int i = ContactsIndex; i < table.Rows.Count; i++)
                {
                    var info = ComputeLocation(table, DutyRule, time, i);
                    persons.Add(info);
                }
                return persons;
            }
            return new List<PersonInfo>();
        }

        private DutyRule getDutyRule(string path)
        {
            var content = IO.ReadAllText(path);
            var rule = JsonSerializer.Deserialize<DutyRule>(content);
            return rule ?? new DutyRule();
        }
        private Dictionary<string, Dutyinfos> getDutyinfosDict(string path)
        {
            try
            {
                var Dutyinfo = File.ReadAllText(path);
                return JsonSerializer.Deserialize<Dictionary<string, Dutyinfos>>(Dutyinfo) ?? new Dictionary<string, Dutyinfos>();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return new Dictionary<string, Dutyinfos>();
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 检查传入的时间是否有效，无效则返回当前时间，反之则返回输入值。
        /// </summary>
        /// <param name="Inputtime"></param>
        /// <returns></returns>
        private DateTime TimeEffective(DateTime Inputtime)
        {
            if (Inputtime == DateTime.MinValue || Inputtime.Year != DateTime.Now.Year)
                return DateTime.Now;
            return Inputtime;
        }
        /// <summary>
        /// 班次名称格式化
        /// </summary>
        /// <param name="duty">格式化操作之前的名称</param>
        /// <returns></returns>
        private static string DutyNameformat(string duty)
        {
            if (duty == "")
                return "**";
            duty = duty.Substring(0, 1);
            return duty + "班";
        }
    }
}

