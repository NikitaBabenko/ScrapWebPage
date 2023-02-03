using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Identity.Client;
using Services.Dto;
using Services.Enums;
using System.Linq;
using System.Runtime.InteropServices;

namespace Services
{
    public class DbService
    {
        private readonly string connectionString;
        public DbService(string connectionString) 
        {
            this.connectionString = connectionString;
        }
        //public void Test()
        //{
        //    using (DAL.AppContext db = new())
        //    {
        //        DAL.Models.Price price = new DAL.Models.Price("Test2", "123", DateTime.Now);

        //        db.Prices.Add(price);
        //        db.SaveChanges();

        //        var test = db.Prices.ToList();
        //        int sdf = 3;
        //    }
        //}

        public async Task SavePrices(IEnumerable<Price> prices, ConflictResolveType conflictResolveType = ConflictResolveType.Update)
        {
            using (DAL.AppContext db = new(connectionString))
            {
                db.Database.SetCommandTimeout(10 * 60);

                var modifiedPrices = prices.Select(MapToDb).ToList();
                //var condTicker = modifiedPrices.Select(mp => mp.Ticker).ToList();
                //var condTimeFrame = modifiedPrices.Select(mp => mp.TimeFrame).ToList();
                //var condDate = modifiedPrices.Select(mp => mp.Date).ToList();
                //var dbPrices = await db.Prices.Where(p => modifiedPrices.Any(mp => mp.Ticker == p.Ticker && mp.TimeFrame == p.TimeFrame && mp.Date.Ticks == p.Date.Ticks)).ToListAsync();
                //var dbPrices = await db.Prices.Where(p => condTicker.Any(c => c == p.Ticker) && condTimeFrame.Any(c => c == p.TimeFrame) && condDate.Any(c => c == p.Date)).ToListAsync();
                var dates = modifiedPrices.Select(m => m.Date).Distinct().ToList();
                var dbPrices = dates.Count > 10 ? db.Prices : db.Prices.Where(p => dates.Contains(p.Date));

                foreach (var modified in modifiedPrices)
                {
                    var existingPrice = await dbPrices.FirstOrDefaultAsync(p => p.Ticker == modified.Ticker && p.TimeFrame == modified.TimeFrame && p.Date == modified.Date);
                    if (existingPrice == null)
                    {
                        db.Prices.Add(modified);
                    }
                    else
                    {
                        switch (conflictResolveType)
                        {
                            case ConflictResolveType.Stop:
                                throw new Exception($"Существует дубликат записи Ticker = {modified.Ticker} TimeFrame = {modified.TimeFrame} Date = {modified.Date}");
                            case ConflictResolveType.Ignore:
                                break;
                            case ConflictResolveType.Update:
                                UpdateProperties(existingPrice, modified);
                                break;
                            default:
                                break;
                        }
                    }
                }

                await db.SaveChangesAsync();
            }
        }

        public async Task UpdateProfitAndDrawdown(IEnumerable<Price> prices)
        {
            using (DAL.AppContext db = new(connectionString))
            {
                var dfsdfsdfsdfs = prices.Where(r => r.Signal != Signal.None).ToList();
                var dsfds = dfsdfsdfsdfs.Where(NeedToUpdateProfitAndDrawdown).ToList();

                foreach (var price in prices.Where(NeedToUpdateProfitAndDrawdown))
                {
                    var existingPrice = await db.BacktestResults.FirstOrDefaultAsync(p => p.Ticker == price.Ticker && p.TimeFrame == price.TimeFrame && p.Date == price.Date);
                    if(existingPrice == null)
                    {
                        throw new Exception($"Не найдена запись для Ticker = {price.Ticker} TimeFrame = {price.TimeFrame} Date = {price.Date}");
                    }
                    UpdateProfitAndDrawdownProperties(existingPrice, price);
                }
                await db.SaveChangesAsync();
            }
        }

        public async Task<List<Price>> GetPrices(DateTime fromDate, DateTime toDate)
        {
            using (DAL.AppContext db = new(connectionString))
            {
                //var allByTime = db.Prices.Where(p => p.Date >= fromDate && p.Date <= toDate).GroupBy(g => new { g.Ticker, g.TimeFrame });
                //int count = allByTime.Count();
                //for (int i = 0; i < count; i++)
                //{
                //    var r = allByTime.Take(1).Skip(i);
                //}
                var retVal = await db.BacktestResults.Where(p => p.Date >= fromDate && p.Date <= toDate).ToListAsync();
                return retVal.Select(MapToDto).ToList();
             }
        }

        private static void UpdateProperties(DAL.Models.Price source, DAL.Models.Price modified)
        {
            source.Close = modified.Close;
            source.High = modified.High;
            source.Low = modified.Low;
            source.Open = modified.Open;
            source.OpenInterest = modified.OpenInterest;
            source.Time = modified.Time;
            source.Volume = modified.Volume;
        }

        private static void UpdateProfitAndDrawdownProperties(DAL.Models.BacktestResults source, Price modified)
        {
            source.Profit = modified.Profit;
            source.Drawdown = modified.Drawdown;
            source.TimeToProfit = modified.TimeToProfit;
            source.TimeToDrawDown = modified.TimeToDrawDown;
        }

        private static DAL.Models.Price MapToDb(Price source)
        {
            DAL.Models.Price retVal = new DAL.Models.Price(source.Ticker, source.TimeFrame, source.Date) 
            { 
                Close = source.Close, 
                High = source.High, 
                Low = source.Low, 
                Open = source.Open, 
                OpenInterest = source.OpenInterest, 
                Time = source.Time, 
                Volume = source.Volume
            };
            return retVal;
        }

        private static Price MapToDto(DAL.Models.BacktestResults source)
        {
            Price retVal = new Price(source.Ticker, source.TimeFrame, source.Date, source.Time, source.Open, source.High, source.Low, source.Close,
                source.Volume, source.OpenInterest);
            retVal.Profit = source.Profit;
            retVal.TimeToProfit = source.TimeToDrawDown;
            retVal.Drawdown = source.Drawdown;
            retVal.TimeToDrawDown = source.TimeToDrawDown;
            retVal.Signal = MapToSignal(source.Signal);
            return retVal;
        }

        private static bool NeedToUpdateProfitAndDrawdown(Price price)
        {
            return price.Profit.HasValue || price.TimeToProfit.HasValue || price.Drawdown.HasValue || price.TimeToDrawDown.HasValue;
        }

        static Enums.Signal MapToSignal(string? source)
        {
            if (source == null)
            {
                return Enums.Signal.None;
            }
            return source.Contains("buy", StringComparison.OrdinalIgnoreCase) ? Enums.Signal.Buy : source.Contains("sell", StringComparison.OrdinalIgnoreCase) ? Enums.Signal.Sell : Enums.Signal.None;
        }
    }
}