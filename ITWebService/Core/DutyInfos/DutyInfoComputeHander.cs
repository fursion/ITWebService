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
            Compute(ref model, index);
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
            //foreach (var item in header)
            //  infos.Add(item);
            #region 修改
            for (int i = 0; i < header.Length; i++)
            {
                if (i == 0)
                    infos.Add(string.Format(header[0], DateTime.Now.ToString("yyyy-MM-dd")));
                else
                    infos.Add(header[i]);
            }
            #endregion
            foreach (var item in locainfos)
            {
                infos.Add("---");
                if (item.Item1 == "IFS国际金融中心")
                    infos.Add(item.Item1 + " 40楼IT服务台");
                else if (item.Item1 == "瑞鑫时代大厦")
                    infos.Add(item.Item1 + "6楼IT服务台");
                else
                    infos.Add(item.Item1);
                infos.Add("---");
                foreach (var info in item.Item2)
                {
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
        /// 计算
        /// </summary>
        private string TemplateCompose(string Temp, PersonInfo info)
        {
            System.Console.WriteLine(Temp, info.Icon, info.DutyTime, info.Name, info.Link);
            return string.Format(Temp, info.Icon, info.DutyTime, info.Name, info.Link);
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
            return AnalysisDutyDatatable(TimeEffective(model.SelectTime), tables[0], ref model, index);

        }
        /// <summary>
        /// 解析班表
        /// </summary>
        /// <param name="time"></param>
        /// <param name="table"></param>
        private ValueTuple<string, List<string>> AnalysisDutyDatatable(DateTime time, DataTable table, ref PersonOnDutyInfoModel model, int index = 0)
        {
            int day = time.Day;
            Console.WriteLine(time);
            int NowMouthDays = Thread.CurrentThread.CurrentUICulture.Calendar.GetDaysInMonth(time.Year, time.Month);
            //获取表中时间定位点(Rows[0],Column[0])
            int ContactsIndex = 2;
            string TimeLocation = table.Rows[0][table.Columns[0]].ToString();
            if (TimeLocation.Contains(time.Year.ToString()) && TimeLocation.Contains(time.Month.ToString()))//判断表是否为当前月
            {
                List<string> infos = new List<string>();
                List<PersonInfo> persons = new List<PersonInfo>();
                for (int i = ContactsIndex; i < table.Rows.Count; i++)
                {
                    var info = ComputeLocation(table, DutyRule, time, i);
                    //排序2.0
                    if (info.IsTrue)
                    {
                        persons.Add(info);
                        //infos.Add(TemplateCompose(PersonInfoTemplate, info));
                    }
                }
                PersonSort2(ref persons);
                infos = FormatInfo(persons);
                //foreach (var p in persons)
                //{
                    //infos.Add(TemplateCompose(PersonInfoTemplate, p));
                //}
                ValueTuple<string, List<string>> Values = new(model.Location[index], infos);
                return Values;
                //ComputeInfos(infos, ref model);
            }
            return new(null, null);
        }
        public List<string> FormatInfo(List<PersonInfo> persons)
        {
            List<string> infos = new List<string>();
            string tmp = "";
            string Target = null;
            foreach (var p in persons)
            {
                if (p.DutyTime == Target)
                {
                    tmp += string.Format("   [{0}]({1})", p.Name, p.Link);
                }
                else
                {
                    if (string.IsNullOrEmpty(tmp))
                    {
                        tmp = TemplateCompose(PersonInfoTemplate, p);
                        Target = p.DutyTime;
                    }
                    else
                    {
                        infos.Add(tmp);
                        tmp = TemplateCompose(PersonInfoTemplate, p);
                        Target = p.DutyTime;
                    }
                }   
            }
            infos.Add(tmp);
            return infos;
        }
        /// <summary>
        /// 排序算法2.0 
        /// </summary>
        private void PersonSort2(ref List<PersonInfo> personInfos)
        {
            if (DutyRule.SortRules == null)
                return;
            var sortRulesCount = DutyRule.SortRules.Keys.Count;
            string[] keys = new string[sortRulesCount];
            int key_index = 0;
            DutyRule.SortRules.Keys.CopyTo(keys, key_index);
            Sort2(DutyRule.SortRules, ref key_index, keys, ref personInfos);
        }
        private void Sort2(Dictionary<string, DutyRuleBody> ruledict, ref int rule_index, string[] rule_keys, ref List<PersonInfo> Infos)
        {
            var SortBlock = ruledict.Count;
            if (ruledict.ContainsKey(rule_keys[rule_index]))
            {
                var rule = ruledict[rule_keys[rule_index]];
                switch (rule.type)
                {
                    case "||": OrderSort(rule_keys[rule_index], ruledict, rule_keys, ref Infos, rule, ref rule_index, SortBlock); break;
                    case "<": MaximumSort(MaximumSortType.Top, rule.ruleVaule[0], rule_keys[rule_index], ruledict, rule_keys, ref Infos, rule, ref rule_index, SortBlock); break;
                    case ">": MaximumSort(MaximumSortType.Bottom, rule.ruleVaule[0], rule_keys[rule_index], ruledict, rule_keys, ref Infos, rule, ref rule_index, SortBlock); break;
                    default: break;
                }
            }
        }
        /// <summary>
        /// 顺序排序
        /// </summary>
        /// <param name="Sortkey"></param>
        /// <param name="ruledict"></param>
        /// <param name="rule_keys"></param>
        /// <param name="infos"></param>
        /// <param name="ruleBody"></param>
        /// <param name="rule_index"></param>
        /// <param name="rules"></param>
        public void OrderSort(string Sortkey, Dictionary<string, DutyRuleBody> ruledict, string[] rule_keys, ref List<PersonInfo> infos, DutyRuleBody ruleBody, ref int rule_index, int rules)
        {
            var ins = new List<int>();
            List<PersonInfo>[] pers = new List<PersonInfo>[ruleBody.ruleVaule.Count];

            for (int key = 0; key < ruleBody.ruleVaule.Count; key++)
            {
                pers[key] = new List<PersonInfo>();
                for (int i = 0; i < infos.Count; i++)
                {
                    if (ruleBody.ruleVaule[key] == typeof(PersonInfo).GetProperty(Sortkey).GetValue(infos[i], null).ToString())
                    {
                        pers[key].Add(infos[i]);
                        ins.Add(i);
                    }
                }
            }
            if (rule_index < rules - 1)
            {
                rule_index++;
                for (int i = 0; i < pers.Length; i++)
                {
                    Sort2(ruledict, ref rule_index, rule_keys, ref pers[i]);
                }
            }
            List<PersonInfo> res = new List<PersonInfo>();
            foreach (var sub in pers)
            {
                foreach (var subc in sub)
                {
                    res.Add(subc);
                }
            }
            //移除已经参与排序的，剩下未命中key的项
            var temp = new List<PersonInfo>();
            foreach (var ind in ins)
            {
                for (int i = 0; i < infos.Count; i++)
                {

                    if (i == ind)
                        continue;
                    else
                        temp.Add(infos[i]);
                }
            }
            var tmp2 = infos;
            infos.Clear(); infos = tmp2;
            //将剩下的未命中的项目添加的返回体的尾部
            foreach (var item in infos)
            {
                res.Add(item);
            }
            infos.Clear();
            infos = res;
        }
        public enum MaximumSortType
        {
            Top,
            Bottom
        }
        private void MaximumSort(MaximumSortType maximumSortType, string Target, string Sortkey, Dictionary<string, DutyRuleBody> ruledict, string[] rule_keys, ref List<PersonInfo> infos, DutyRuleBody ruleBody, ref int rule_index, int rules)
        {
            var TargetIndex = new List<int>();

            List<PersonInfo> tars = new List<PersonInfo>();
            for (int i = 0; i < infos.Count; i++)
            {
                if (infos[i].GetType().GetProperty(Sortkey).GetValue(infos[i], null).ToString().Contains(ruleBody.ruleVaule[0].Nameformat()))
                {
                    tars.Add(infos[i]);
                    TargetIndex.Add(i);
                }
            }
            foreach (var item in TargetIndex)
            {
                infos.RemoveAt(item);
            }
            if (rule_index < rules - 1)
            {
                rule_index++;
                Sort2(ruledict, ref rule_index, rule_keys, ref tars);
            }
            List<PersonInfo> res = new List<PersonInfo>();
            if (maximumSortType == MaximumSortType.Top)
            {
                foreach (var item in tars)
                {
                    res.Add(item);
                }
                foreach (var item in infos)
                {
                    res.Add(item);
                }
                infos.Clear();
                infos = res;
            }
            else
            {
                foreach (var item in tars)
                {
                    infos.Add(item);
                }
            }
        }

        /// <summary>
        /// 人员排序
        /// </summary>
        /// <param name="infos"></param>
        [Obsolete("此方法遗弃", true)]
        private void PersonSort(ref List<string> infos)
        {
            if (DutyRule.SortRules == null)
                return;
            var sortRulesCount = DutyRule.SortRules.Keys.Count;
            string[] keys = new string[sortRulesCount];
            int key_index = 0;
            DutyRule.SortRules.Keys.CopyTo(keys, key_index);
            //Sort(DutyRule.SortRules, ref key_index, keys, ref infos);
        }

        /// <summary>
        /// 递归执行排序规则
        /// </summary>
        /// <param name="ruledict"></param>
        /// <param name="dict_index"></param>
        /// <param name="dict_keys"></param>
        /// <param name="Infos"></param>
        [Obsolete("此方法遗弃", true)]
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

