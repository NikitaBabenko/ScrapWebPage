using Services.Dto;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class CalcService
    {
        public void CalcRows(IEnumerable<Price> rows, int rowsPeriod)
        {
            var rowsToCalc = rows.Where(r => r.Signal != Enums.Signal.None).ToList();
            for (int i = 0; i < rowsToCalc.Count; i++)
            {
                var period = rows.Where(r => r.Date >= rowsToCalc[i].Date
                        && r.Ticker == rowsToCalc[i].Ticker
                        && r.TimeFrame == rowsToCalc[i].TimeFrame)
                    .OrderBy(r => r.Date)
                    .Take(rowsPeriod)
                    .ToList();

                CalcPeriod(rowsToCalc[i], period);
                rowsToCalc[i].Profit *= 100;
                rowsToCalc[i].Drawdown *= 100;
            }
        }

        private static void CalcPeriod(Price row, List<Price> rows)
        {
            var maxHighRow = rows.OrderByDescending(r => r.High).First();
            var minLowRow = rows.OrderBy(r => r.Low).First();
            int rowNum = GetRowNumber(rows, row);
            int maxHighRowNum = GetRowNumber(rows, maxHighRow);
            int minLowRowNum = GetRowNumber(rows, minLowRow);
            switch (row.Signal)
            {
                case Enums.Signal.Sell:
                    row.Profit = (row.Close - minLowRow.Low) / row.Close;
                    row.TimeToProfit = minLowRowNum - rowNum;

                    row.Drawdown = (row.Close - maxHighRow.High) / row.Close;
                    row.TimeToDrawDown = maxHighRowNum - rowNum;
                    break;
                case Enums.Signal.Buy:
                    row.Profit = (maxHighRow.High - row.Close) / row.Close;
                    row.TimeToProfit = maxHighRowNum - rowNum;

                    row.Drawdown = (minLowRow.Low - row.Close) / row.Close;
                    row.TimeToDrawDown = minLowRowNum - rowNum;
                    break;
                default:
                    break;
            }
        }

        private static int GetRowNumber(List<Price> prices, Price price)
        {
            return prices.FindIndex(a => a == price);
        }



        public void CalcExcelRows(IEnumerable<ExcelRow> rows, int rowsPeriod)
        {
            foreach (var rowCalc in rows.Where(r => r.Signal != Enums.Signal.None).ToList())
            {
                var period = rows.Where(r => r.rowNumber > rowCalc.rowNumber
                        && r.Ticker == rowCalc.Ticker
                        && r.TimeFrame == rowCalc.TimeFrame)
                    .Take(rowsPeriod)
                    .ToList();

                CalcPeriod(rowCalc, period);
                rowCalc.Profit *= 100;
                rowCalc.Drawdown *= 100;
            } 
        }

        private static void CalcPeriod(ExcelRow row, List<ExcelRow> rows)
        {
            var maxHighRow = rows.OrderByDescending(r => r.High).First();
            var minLowRow = rows.OrderBy(r => r.Low).First();
            switch (row.Signal)
            {
                case Enums.Signal.Sell:
                    row.Profit = (row.Close - minLowRow.Low) / row.Close;
                    row.TimeToProfit = minLowRow.rowNumber - row.rowNumber;

                    row.Drawdown = (row.Close - maxHighRow.High) / row.Close;
                    row.TimeToDrawDown = maxHighRow.rowNumber - row.rowNumber;
                    break;
                case Enums.Signal.Buy:
                    row.Profit = (maxHighRow.High - row.Close) / row.Close;
                    row.TimeToProfit = maxHighRow.rowNumber - row.rowNumber;
                    
                    row.Drawdown = (minLowRow.Low - row.Close) / row.Close;
                    row.TimeToDrawDown = minLowRow.rowNumber - row.rowNumber;
                    break;
                default:
                    break;
            }
        }



    }
}
