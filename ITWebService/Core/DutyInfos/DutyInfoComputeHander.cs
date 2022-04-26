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
    public static class DutyTool
    {
        /// <summary>
        /// 姓名格式化
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public static string Nameformat(this string Name)
        {      
            if (Name.Length == 2)
            {
                Name = Name[0] + "  " + Name[1];
            }
            return Name;
        }
    }
    public class DutyInfoComputeHander : IDisposable
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
        private string PersonInfoTemplate;
        private DutyRule DutyRule { get; set; }
        private Dictionary<string, string> PersonLinksDict { get; set; }
        private Dictionary<string, Dutyinfos> DutyinfosDict { get; set; }
        public DutyInfoComputeHander(ref PersonOnDutyInfoModel model, int index = 0)
        {
            Compute(ref model,index); 
        }
        #region  测试算法
        public void Compute(ref PersonOnDutyInfoModel model, int factor = 0)
        {
            //
            List<ValueTuple<string, List<string>>> locainfos = new();
            for (int i = 0; i < factor; i++)
            {
                var Values = Computehander(ref model, i);
                locainfos.Add(Values);
            }
            string headerpath = Path.Combine(ConfigCore.WebRootPath, ConfigCore.GetConfigItem<DutyConfig>().FolderPath, model.Location[0], ConfigCore.GetConfigItem<DutyConfig>().TempHeader);
            string footerpath = Path.Combine(ConfigCore.WebRootPath, ConfigCore.GetConfigItem<DutyConfig>().FolderPath, model.Location[0], ConfigCore.GetConfigItem<DutyConfig>().TempTail);

            string[] header = IO.ReadAllLines(headerpath);
            string[] footer = IO.ReadAllLines(footerpath);
            List<string> infos = new();
            foreach (var item in header)
                infos.Add(item);
            foreach (var item in locainfos)
            {
                infos.Add("---");
                infos.Add(item.Item1);
                infos.Add("---");
                foreach(var info in item.Item2){
                    infos.Add(info);
                }
            }
            foreach (var item in footer)
                infos.Add(item);
            model.Infos = infos;
            //
        }
        public ValueTuple<string, List<string>> Computehander(ref PersonOnDutyInfoModel model, int index)
        {
            var rulefilepath = Path.Combine(ConfigCore.WebRootPath, ConfigCore.GetConfigItem<DutyConfig>().FolderPath, model.Location[index], "Rules.json");//生成班表文件路径
            var Dutyinfosfilepath = Path.Combine(ConfigCore.WebRootPath, ConfigCore.GetConfigItem<DutyConfig>().FolderPath, model.Location[index], "DutyInfo.json");
            PersonInfoTemplate = IO.ReadAllText(Path.Combine(ConfigCore.WebRootPath, ConfigCore.GetConfigItem<DutyConfig>().FolderPath, model.Location[index], "TempInfo.txt"));
            DutyRule = getDutyRule(rulefilepath);
            DutyinfosDict = getDutyinfosDict(Dutyinfosfilepath);
            model.Infos = new List<string>();
            return ComputeDutyInfos(ref model, index);
        }
        #endregion
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
        private ValueTuple<string, List<string>> ComputeDutyInfos(ref PersonOnDutyInfoModel model, int index = 0)
        {
            var dutyfilepath = Path.Combine(ConfigCore.WebRootPath, ConfigCore.GetConfigItem<DutyConfig>().FolderPath, model.Location[index], $"{model.Location[index]}duty.xlsx");//生成班表文件路径

            var tables = getDataTable(dutyfilepath);
            return AnalysisDutyDatatable(TimeEffective(model.SelectTime), tables[0], ref model,index);

        }
        /// <summary>
        /// 解析班表
        /// </summary>
        /// <param name="time"></param>
        /// <param name="table"></param>
        private ValueTuple<string, List<string>> AnalysisDutyDatatable(DateTime time, DataTable table, ref PersonOnDutyInfoModel model, int index = 0)
        {
            int day = time.Day;
            int NowMouthDays = Thread.CurrentThread.CurrentUICulture.Calendar.GetDaysInMonth(time.Year, time.Month);
            //获取表中时间定位点(Rows[0],Column[0])
            int ContactsIndex = 2;
            string TimeLocation = table.Rows[0][table.Columns[0]].ToString();
            if (TimeLocation.Contains(time.Year.ToString()) && TimeLocation.Contains(time.Month.ToString()))//判断表是否为当前月
            {
                List<string> infos = new List<string>();
                for (int i = ContactsIndex; i < table.Rows.Count; i++)
                {
                    var info = ComputeLocation(table, DutyRule, time, i);
                    if (info.IsTrue)
                    {
                        infos.Add(TemplateCompose(PersonInfoTemplate, info));
                    }
                }
                PersonSort(ref infos);
                ValueTuple<string, List<string>> Values = new(model.Location[index], infos);
                return Values;
                //ComputeInfos(infos, ref model);
            }
            return new(null, null);
        }
        private void PersonSort(ref List<string> infos)
        {
            if (DutyRule.SortRules == null)
                return;
            var sortRulesCount = DutyRule.SortRules.Keys.Count;
            string[] keys = new string[sortRulesCount];
            int key_index = 0;
            DutyRule.SortRules.Keys.CopyTo(keys, key_index);
            Sort(DutyRule.SortRules, ref key_index, keys, ref infos);
            //var method = this.GetType().GetMethod($"Sort{keys[key_index]}");
            //method.Invoke(this, new object[] { model, DutyRule.SortRules[keys[key_index]], key_index });
        }
        /// <summary>
        /// 递归执行排序规则
        /// </summary>
        /// <param name="ruledict"></param>
        /// <param name="dict_index"></param>
        /// <param name="dict_keys"></param>
        /// <param name="Infos"></param>
        private void Sort(Dictionary<string, List<string>> ruledict, ref int dict_index, string[] dict_keys, ref List<string> Infos)
        {
            var max = ruledict.Keys.Count;
            if (ruledict.ContainsKey(dict_keys[dict_index]))
            {
                var list = Infos;
                var keyValueCount = ruledict[dict_keys[dict_index]].Count;
                var Values = ruledict[dict_keys[dict_index]];
                List<string>[] lists = new List<string>[keyValueCount];
                for (int i = 0; i < lists.Length; i++)
                    lists[i] = new List<string>();
                for (int ruleindex = 0; ruleindex < Values.Count; ruleindex++)
                {
                    for (int index = 0; index < list.Count; index++)
                    {
                        if (list[index].Contains(Values[ruleindex]))
                            lists[ruleindex].Add(list[index]);
                    }
                    if (dict_index < max - 1)
                    {
                        dict_index++;
                        Sort(ruledict, ref dict_index, dict_keys, ref lists[ruleindex]);
                    }
                }
                List<string> rlist = new();
                foreach (var Sublist in lists)
                {
                    foreach (var item in Sublist)
                    {
                        rlist.Add(item);
                    }
                }
                Infos = rlist;
            }
            else if (dict_index < max - 1)
            {
                dict_index++;
                Sort(ruledict, ref dict_index, dict_keys, ref Infos);
            }
        }
        /// <summary>
        /// 文本汇总根据模板生成最终信息
        /// </summary>
        /// <param name="infoBody"></param>
        /// <param name="model"></param>
        private void ComputeInfos(List<string> infoBody, ref PersonOnDutyInfoModel model, int index = 0)
        {
            string headerpath = Path.Combine(ConfigCore.WebRootPath, ConfigCore.GetConfigItem<DutyConfig>().FolderPath, model.Location[index], ConfigCore.GetConfigItem<DutyConfig>().TempHeader);
            string footerpath = Path.Combine(ConfigCore.WebRootPath, ConfigCore.GetConfigItem<DutyConfig>().FolderPath, model.Location[index], ConfigCore.GetConfigItem<DutyConfig>().TempTail);

            string[] header = IO.ReadAllLines(headerpath);
            string[] footer = IO.ReadAllLines(footerpath);
            List<string> infos = new();
            foreach (var item in header)
                infos.Add(item);
            foreach (var item in infoBody)
                infos.Add(item);
            foreach (var item in footer)
                infos.Add(item);
            model.Infos = infos;
        }
        /// <summary>
        /// 获取对应规则实例
        /// </summary>
        private DutyRule getDutyRule(string path)
        {
            var content = IO.ReadAllText(path);
            //Console.WriteLine($"rule path : {path} content : {content}");
            var rule = JsonSerializer.Deserialize<DutyRule>(content);
            return rule;
        }
        private Dictionary<string, Dutyinfos> getDutyinfosDict(string path)
        {
            try
            {
                var Dutyinfo = File.ReadAllText(path);
                return JsonSerializer.Deserialize<Dictionary<string, Dutyinfos>>(Dutyinfo);
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
                return null;
            duty = duty.Substring(0, 1);
            return duty + "班";
        }
    }
}

