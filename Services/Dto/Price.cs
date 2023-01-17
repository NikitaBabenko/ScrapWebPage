namespace Services.Dto
{
    public record Price(string Ticker, string TimeFrame, DateTime Date, int? Time, double? Open, double? High, double? Low, double? Close, double? Volume, int? OpenInterest)
    {
    }
}
