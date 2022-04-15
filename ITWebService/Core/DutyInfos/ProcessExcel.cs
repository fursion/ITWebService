/*  fursion@fursion.cn
 *  班表解析类
 * 
 * 
 * 
 */
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Data.OleDb;
using NPOI;
using NPOI.SS.UserModel;
using System.Data;
using System.Text.Json;
namespace ITWebService.Core
{
    using ShiftsRules = ValueTuple<int, int[], string, string, string, string>;//班表规则模板，item1为排班周期，item2为索引，item3为条件值,item4为命中规则的值，item5为默认值
    using Per_Info_OnTable = ValueTuple<string, string, int, int>;
    using Today_Per_infos = List<ValueTuple<string, string, int, int>>;
    public static class ExcelTools
    {
        /// <summary>
        /// excel读取
        /// </summary>
        /// <param name="Excelfilepath">文件路径</param>
        /// <param name="sheetName">表名</param>
        /// <param name="IsFirstRowColumnName">第一行是否是列名</param>
        /// <returns></returns>
        public static DataTable ReadExcel(string Excelfilepath, string sheetName = null, bool IsFirstRowColumnName = true)
        {
            DataTable dataTable = new DataTable();
            FileStream ExcelSr = new FileStream(Excelfilepath, FileMode.Open, FileAccess.Read);
            ISheet sheet = null;
            int startRow = 0;
            try
            {
                IWorkbook workbook = WorkbookFactory.Create(ExcelSr);
                if (!string.IsNullOrEmpty(sheetName))
                {
                    sheet = workbook.GetSheet(sheetName);
                    if (null == sheet)
                    {
                        sheet = workbook.GetSheetAt(0);
                    }
                }
                else
                    sheet = workbook.GetSheetAt(0);
                if (null != sheet)
                {
                    IRow firstRow = sheet.GetRow(0);
                    int cellCount = firstRow.LastCellNum;
                    if (IsFirstRowColumnName)
                    {
                        for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                        {
                            ICell cell = firstRow.GetCell(i);
                            if (null != cell)
                            {
                                cell.SetCellType(CellType.String);
                                string cellvalue = cell.StringCellValue;
                                if (null != cellvalue)
                                {
                                    DataColumn column = new DataColumn(cellvalue);
                                    dataTable.Columns.Add(column);
                                }
                            }
                        }
                        startRow = sheet.FirstRowNum + 1;
                    }
                    else
                    {
                        startRow = sheet.FirstRowNum;
                    }
                    int rowCount = sheet.LastRowNum;
                    for (int i = startRow; i <= rowCount; ++i)
                    {
                        IRow row = sheet.GetRow(i);
                        if (null == row)
                            continue;
                        DataRow dataRow = dataTable.NewRow();
                        for (int j = row.FirstCellNum; j < cellCount; ++j)
                        {
                            if (null != row.GetCell(j))
                            {
                                dataRow[j] = row.GetCell(j).ToString();
                            }
                        }
                        dataTable.Rows.Add(dataRow);
                    }
                }
                ExcelSr.Dispose();
                return dataTable;
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
                return null;
            }
        }
        /// <summary>
        /// 遍历班表，查询当天值班人员，生成在班信息
        /// </summary>
        /// <param name="table"></param>
        public static IEnumerable<string> Traversal_duty_Table(DataTable table, DateTime dateTime)
        {
            int year = dateTime.Year;
            int month = dateTime.Month;
            int day = dateTime.Day;
            int count = table.Rows.Count;
            List<string> TodayDutyinfo = new List<string>();
            /// <summary>
            /// 姓名，班次，在班表中所在行，日期
            /// </summary>
            Today_Per_infos tuple_duty = new();
            for (int index = 1; index < count; index++)
            {
                var user = table.Rows[index][0].ToString();//获取人员名字
                var item = table.Rows[index][day].ToString();//班次信息
                if (IsMask_Item(item)||!item.StringEffective())//剔除掉屏蔽列表中班次
                    continue;
                item = DutyNameformat(item);//班次名称格式化
                if (DutyInfo.Dutyinfo_dict.ContainsKey(item))
                {
                    var tuple = new Per_Info_OnTable(user, item, index, day);//在班人员在班表中的源信息
                    tuple_duty.Add(tuple);//将源信息加入源信息列表
                }
            }
            Sortdyty(table, ref tuple_duty);
            return Createinfo(table, tuple_duty, dateTime);
        }
        /// <summary>
        /// 班次名称格式化
        /// </summary>
        /// <param name="duty">格式化操作之前的名称</param>
        /// <returns></returns>
        public static string DutyNameformat(string duty)
        {
            duty = duty.Substring(0, 1);
            return duty + "班";
        }
        /// <summary>
        /// 判断是否是屏蔽项
        /// </summary>
        /// <param name="duty"></param>
        /// <returns></returns>
        public static bool IsMask_Item(string duty)
        {
            if (duty.Contains('初') && duty.Contains('白'))
                return true;
            return false;
        }
        /// <summary>
        /// 检验字符串是否有效
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool StringEffective(this string str)
        {
            if (str == ""||null == str)
                return false;
            return true;
        }
        /// <summary>
        /// 生成个人在班信息项
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="tuples"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static IEnumerable<string> Createinfo(DataTable dataTable, Today_Per_infos tuples, DateTime dateTime)
        {
            List<string> TodayDutyinfo = new();
            List<Tuple<string, string, int, int>> tuple_duty = new List<Tuple<string, string, int, int>>();
            //获取这个月的天数
            int NowMouthDays = System.Threading.Thread.CurrentThread.CurrentUICulture.Calendar.GetDaysInMonth(dateTime.Year, dateTime.Month);
            return locationJudge(new ShiftsRules(4, new int[] { 2 }, "白", "15F", "40F", "休"), dataTable, tuples, dateTime.Day, NowMouthDays);
        }
        /// <summary>
        /// 判断值班地点，并写入样式
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="table"></param>
        /// <param name="infos"></param>
        /// <param name="select_index"></param>
        /// <param name="days"></param>
        /// <returns></returns>
        private static List<string> locationJudge(ShiftsRules rule, DataTable table, Today_Per_infos infos, int select_index, int days)
        {

            List<string> todayinfos = new();
            foreach (var item in infos)
            {
                int rest_index = 0;
                string Temp = DutyInfo.Templatedict["班次信息"];
                bool istar = true;
                if (select_index <= days - rule.Item1)
                {
                    for (int i = select_index; i < days; i++)
                    {
                        if (table.Rows[item.Item3][i].ToString() == rule.Item6)
                        {
                            rest_index = i;
                            break;
                        }
                    }
                    foreach (var r in rule.Item2)
                    {
                        istar &= rest_index - select_index == rule.Item1 - r;
                    }
                }
                else
                {
                    for (int i = select_index; i > 0; i--)
                    {
                        if (table.Rows[item.Item3][i].ToString() == rule.Item6)
                        {
                            rest_index = i;
                            break;
                        }
                    }
                    foreach (var r in rule.Item2)
                    {
                        istar &= select_index - rest_index == r;
                    }
                }
                bool isMask = false;
                for (int s = 0; s < rule.Item3.Length; s++)
                {
                    isMask |= item.Item2.Contains(rule.Item3[s]);
                }
                if (istar && isMask)
                {
                    formatinfo(ref Temp, item, rule.Item4);
                    todayinfos.Insert(0, Temp);
                }
                else
                {
                    formatinfo(ref Temp, item, rule.Item5);
                    todayinfos.Add(Temp);
                }
            }
            return todayinfos;
        }
        /// <summary>
        /// 格式化填充信息
        /// </summary>
        /// <param name="temp"></param>
        /// <param name="tuple"></param>
        /// <param name="location">位置</param>
        public static void formatinfo(ref string temp, Per_Info_OnTable tuple, string location = null)
        {
            if (null != location)
                temp = string.Format(temp, DutyInfo.Dutyinfo_dict[tuple.Item2].icon, tuple.Item2, location, DutyInfo.Dutyinfo_dict[tuple.Item2].Timeslot, Nameformat(tuple.Item1), DutyInfo.link_dict[tuple.Item1]);
            else
                temp = string.Format(temp, DutyInfo.Dutyinfo_dict[tuple.Item2].icon, tuple.Item2, DutyInfo.Dutyinfo_dict[tuple.Item2].Place, DutyInfo.Dutyinfo_dict[tuple.Item2].Timeslot, Nameformat(tuple.Item1), DutyInfo.link_dict[tuple.Item1]);
        }
        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="table"></param>
        /// <param name="tuples"></param>
        public static void Sortdyty(DataTable table, ref List<Per_Info_OnTable> tuples)
        {
            List<Per_Info_OnTable> tuple_duty = new();
            foreach (var item in tuples)
            {
                if (item.Item2.Contains('白'))
                    tuple_duty.Insert(0, item);
                else tuple_duty.Add(item);
            }
            tuples = tuple_duty;

        }
        /// <summary>
        /// 姓名格式化
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public static string Nameformat(string Name)
        {
            if (Name.Length == 2)
            {
                Name = Name[0] + "  " + Name[1];
            }
            return Name;
        }
    }
}
