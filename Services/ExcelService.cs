using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;
using Services.Dto;

namespace Services
{
    public class ExcelService
    {
        //public List<ExcelRow> LoadExcel(string filePath)
        //{
        //    Application oExcel = new Application();
        //    Workbook WB = oExcel.Workbooks.Open(filePath);
        //    Worksheet wks = (Worksheet)WB.Worksheets[1];
        //    List<ExcelRow> excelRows = new List<ExcelRow>();
        //    foreach (Microsoft.Office.Interop.Excel.Range row in wks.Rows)
        //    {
        //        excelRows.Add(MapToExcelRow(row));
        //    }
        //    return excelRows;
        //}

        public void SaveProfitDrawdown(string filePath, List<ExcelRow> rows)
        {
            XSSFWorkbook hssfwb;
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
            {
                hssfwb = new XSSFWorkbook(fs);

                ISheet sheet = hssfwb.GetSheetAt(0);

                var headerCells = sheet.GetRow(0);


                //IRow HeaderRow = sheet.CreateRow(0);

                ////Create The Actual Cells
                //CreateCell(HeaderRow, 0, "Batch Name", null);
                //CreateCell(HeaderRow, 1, "RuleID", null);

                //ICell Cell = CurrentRow.CreateCell(CellIndex);
                //Cell.SetCellValue("Profit");

                headerCells.CreateCell(headerCells.LastCellNum).SetCellValue("Profit");
                headerCells.CreateCell(headerCells.LastCellNum).SetCellValue("Drawdown");
                headerCells.CreateCell(headerCells.LastCellNum).SetCellValue("TimeToProfit");
                headerCells.CreateCell(headerCells.LastCellNum).SetCellValue("TimeToDrawDown");

                foreach (var row in rows.Where(r => r.Signal != Enums.Signal.None))
                {
                    var curRow = sheet.GetRow(row.rowNumber);

                    curRow.CreateCell(curRow.LastCellNum).SetCellValue(Utils.GetProcents(row.Profit.Value));
                    curRow.CreateCell(curRow.LastCellNum).SetCellValue(Utils.GetProcents(row.Drawdown.Value));
                    curRow.CreateCell(curRow.LastCellNum).SetCellValue(row.TimeToProfit.Value);
                    curRow.CreateCell(curRow.LastCellNum).SetCellValue(row.TimeToDrawDown.Value);
                }

                for (int i = 1; i < sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                }

                //hssfwb.Write(fs);

                //FileStream xfile = new FileStream(Path.Combine(dldir, filename), FileMode.Create, System.IO.FileAccess.Write);
                //hssfwb.Write(fs);
                //fs.Close();
                //hssfwb.Close();
            }


            var f = filePath.Split('.');
            f[f.Length - 2] += "_processed";
            string newPath = string.Join(".", f);
            using (FileStream fs = new FileStream(newPath, FileMode.Create, FileAccess.Write))
            {
                hssfwb.Write(fs);
                fs.Close();
            }

        }

        //static void WriteExcel()
        //{
        //    List<UserDetails> persons = new List<UserDetails>()
        //    {
        //        new UserDetails() {ID="1001", Name="ABCD", City ="City1", Country="USA"},
        //        new UserDetails() {ID="1002", Name="PQRS", City ="City2", Country="INDIA"},
        //        new UserDetails() {ID="1003", Name="XYZZ", City ="City3", Country="CHINA"},
        //        new UserDetails() {ID="1004", Name="LMNO", City ="City4", Country="UK"},
        //   };

        //    // Lets converts our object data to Datatable for a simplified logic.
        //    // Datatable is most easy way to deal with complex datatypes for easy reading and formatting.

        //    DataTable table = (DataTable)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(persons), (typeof(DataTable)));
        //    var memoryStream = new MemoryStream();

        //    using (var fs = new FileStream("Result.xlsx", FileMode.Create, FileAccess.Write))
        //    {
        //        IWorkbook workbook = new XSSFWorkbook();
        //        ISheet excelSheet = workbook.CreateSheet("Sheet1");

        //        List<String> columns = new List<string>();
        //        IRow row = excelSheet.CreateRow(0);
        //        int columnIndex = 0;

        //        foreach (System.Data.DataColumn column in table.Columns)
        //        {
        //            columns.Add(column.ColumnName);
        //            row.CreateCell(columnIndex).SetCellValue(column.ColumnName);
        //            columnIndex++;
        //        }

        //        int rowIndex = 1;
        //        foreach (DataRow dsrow in table.Rows)
        //        {
        //            row = excelSheet.CreateRow(rowIndex);
        //            int cellIndex = 0;
        //            foreach (String col in columns)
        //            {
        //                row.CreateCell(cellIndex).SetCellValue(dsrow[col].ToString());
        //                cellIndex++;
        //            }

        //            rowIndex++;
        //        }
        //        workbook.Write(fs);
        //    }

        //}


        private void CreateCell(IRow CurrentRow, int CellIndex, string Value, HSSFCellStyle Style)
        {
            var Cell = CurrentRow.CreateCell(CellIndex);
            Cell.SetCellValue(Value);
            //Cell.CellStyle = Style;
        }

        public List<ExcelRow> LoadExcel(string filePath)
        {
            XSSFWorkbook hssfwb;
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                hssfwb = new XSSFWorkbook(fs);
            }

            ISheet sheet = hssfwb.GetSheetAt(0);
            List<ExcelRow> excelRows = new List<ExcelRow>();

            for (int i = 1; i < sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);
                excelRows.Add(MapToExcelRow(row));
            }
            return excelRows.Where(e => e != null).ToList();
        }

        private static ExcelRow? MapToExcelRow(IRow? row)
        {
            if (row == null)
            {
                return null;
            }

            Enums.Signal signal = Enums.Signal.None;
            int offset = 0;

            switch (row.Cells.Count)
            {
                case 7:
                    offset = 1;
                    break;
                case 8:
                    signal = MapToSignal(row.Cells[3].StringCellValue);
                    break;
                case 11:
                    offset = 1;
                    break;
                case 12:
                    signal = MapToSignal(row.Cells[3].StringCellValue);
                    break;
                default:
                    break;
            }

            return new ExcelRow(
                row.RowNum,
                row.Cells[0].DateCellValue, //date
                row.Cells[1].StringCellValue, //ticker
                row.Cells[2].StringCellValue, //timeframe
                signal, //signal
                row.Cells[4 - offset].NumericCellValue,
                row.Cells[5 - offset].NumericCellValue,
                row.Cells[6 - offset].NumericCellValue,
                row.Cells[7 - offset].NumericCellValue


                //                Utils.GetDecimal(row.Cells[4].ToString()),
                //Utils.GetDecimal(row.Cells[5].ToString()),
                //Utils.GetDecimal(row.Cells[6].ToString()),
                //Utils.GetDecimal(row.Cells[7].ToString()),

                //null,
                //null,
                //null,
                //null
                //row[8].ToString(), 
                //row[9].ToString(), 
                //row[10].ToString(), 
                //row[11].ToString()
                );
            //date	ticker	timeframe	signal	open	high	low	close	
            //date	    ticker	    timeframe	signal	        open	high	low	    close   ||  profit  drawdown TimeToProfit   TimeToDrawDown
            //10/1/2009	ACCO.US	    D           buy TrendRSI	6.09	6.16	5.82	5.83    ||  35%     -45%
            //                                  sell TrendRSI
        }

        static Enums.Signal MapToSignal(string source)
        {
            return source.Contains("buy", StringComparison.OrdinalIgnoreCase) ? Enums.Signal.Buy : source.Contains("sell", StringComparison.OrdinalIgnoreCase) ? Enums.Signal.Sell : Enums.Signal.None;
        }

    }
}
