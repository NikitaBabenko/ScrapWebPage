using System.ComponentModel.DataAnnotations;

namespace DAL.Models
{
    public class Price
    {
        public Price(string ticker, string timeFrame, DateTime date)
        {
            Ticker = ticker;
            TimeFrame = timeFrame;
            Date = date;
        }

        public string Ticker { get; set; }
        public string TimeFrame { get; set; }
        public DateTime Date { get; set; }
        public int? Time { get; set; }
        public double? Open { get; set; }
        public double? High { get; set; }
        public double? Low { get; set; }
        public double? Close { get; set; }
        public double? Volume { get; set; }
        public int? OpenInterest { get; set; }
    }
}
