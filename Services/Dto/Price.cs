using Services.Enums;

namespace Services.Dto
{
    public record Price(string Ticker, string TimeFrame, DateTime Date, int? Time, double? Open, double? High, double? Low, double? Close, double? Volume, int? OpenInterest)
        //double? Profit, double? Drawdown, int? TimeToProfit, int? TimeToDrawDown, Signal Signal)
    {
        public double? Profit { get; set; }
        public double? Drawdown { get; set; }
        public int? TimeToProfit { get; set; }
        public int? TimeToDrawDown { get; set; }
        public Signal Signal { get; set; }
        
    }
}
