using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Data.OleDb;
using NPOI;
using NPOI.SS.UserModel;
using System.Data;
using System.Text.Json;
namespace ITWebService.Core.Tools
{
    public static class ExcelTools
    {
        public static List<DataTable>ExcelToDatatable(string Excelfilepath)
        {
            List<DataTable> tables = new List<DataTable>();
            FileStream ExcelSr = new(Excelfilepath, FileMode.Open, FileAccess.Read);
            ISheet sheet = null;
            int startRow = 0;
            try
            {
                IWorkbook workbook = WorkbookFactory.Create(ExcelSr);
                var SheetsNumber = workbook.NumberOfSheets;
                if (SheetsNumber == 0)
                    return null;
                for(int sheetindex = 0; sheetindex < SheetsNumber; sheetindex++)
                {
                    DataTable dataTable = new DataTable();
                    sheet = workbook.GetSheetAt(sheetindex);
                    if (null != sheet)
                    {
                        IRow firstRow = sheet.GetRow(0);
                        if (firstRow == null)
                            continue;
                        int cellCount = firstRow.LastCellNum;
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
                        startRow = sheet.FirstRowNum;
                        int rowCount = sheet.LastRowNum;
                        for (int i = startRow; i <=rowCount; i++)
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
                    tables.Add(dataTable);
                }
                
                ExcelSr.Dispose();
                return tables;
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
                return null;
            }
        }
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
        public static void PrintTable(this DataTable table)
        {
            if (null != table)
            {
                foreach (DataRow row in table.Rows)
                {
                    foreach (DataColumn col in table.Columns)
                    {
                        Console.Write(row[col]);
                        Console.Write(" | ");
                    }
                    Console.WriteLine("");
                }
            }
        }
    }
}

