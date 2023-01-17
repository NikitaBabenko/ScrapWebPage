using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Services.Dto;
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

        public async Task SaveOrUpdatePrices(IEnumerable<Price> prices)
        {
            using (DAL.AppContext db = new(connectionString))
            {
                var modifiedPrices = prices.Select(MapToDb).ToList();
                //var condTicker = modifiedPrices.Select(mp => mp.Ticker).ToList();
                //var condTimeFrame = modifiedPrices.Select(mp => mp.TimeFrame).ToList();
                //var condDate = modifiedPrices.Select(mp => mp.Date).ToList();
                //var dbPrices = await db.Prices.Where(p => modifiedPrices.Any(mp => mp.Ticker == p.Ticker && mp.TimeFrame == p.TimeFrame && mp.Date.Ticks == p.Date.Ticks)).ToListAsync();
                //var dbPrices = await db.Prices.Where(p => condTicker.Any(c => c == p.Ticker) && condTimeFrame.Any(c => c == p.TimeFrame) && condDate.Any(c => c == p.Date)).ToListAsync();
                foreach (var modified in modifiedPrices)
                {
                    var existingPrice = await db.Prices.FindAsync(modified.Ticker, modified.TimeFrame, modified.Date);
                    if(existingPrice == null)
                    {
                        db.Prices.Add(modified);
                    }
                    else
                    {
                        UpdateProperties(existingPrice, modified);
                    }
                }
                await db.SaveChangesAsync();
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
    }
}