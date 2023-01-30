using Services.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Dto
{
    //public record ExcelRow(DateTime Date, string Ticker, string TimeFrame, Signal Signal, decimal Open, decimal High, decimal Low, decimal Close, decimal? Profit, decimal? Drawdown, int? TimeToProfit, int? TimeToDrawDown)
    public record ExcelRow(int rowNumber, DateTime Date, string Ticker, string TimeFrame, Signal Signal, double Open, double High, double Low, double Close)
    {
        //date	    ticker	    timeframe	signal	        open	high	low	    close   ||  profit  drawdown TimeToProfit   TimeToDrawDown
        //10/1/2009	ACCO.US	    D           buy TrendRSI	6.09	6.16	5.82	5.83    ||  35%     -45%
        //                                  sell TrendRSI

        public double? Profit { get; set; }
        public double? Drawdown { get; set; }
        public double? TimeToProfit { get; set; }
        public double? TimeToDrawDown { get; set; }
    }
}
