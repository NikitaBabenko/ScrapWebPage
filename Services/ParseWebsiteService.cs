using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.SqlServer.Server;
using Services.Dto;
using Services.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Services
{
    public class ParseWebsiteService
    {
        public async Task<List<Price>> GetPrices(SourceType sourceType, DateTime? date)
        {
            List<Price> retVal = new List<Price>();

            using (WebClient client = new())
            {
                var url = GetUrl(sourceType, date);
                string htmlCode = await client.DownloadStringTaskAsync(url);

                var parser = new HtmlParser();
                var doc = await parser.ParseDocumentAsync(htmlCode);

                //var datePicker = doc.QuerySelector("input.mfd-input-date");
                //string date = datePicker.Attributes["value"].Value;



                var mainTable = doc.QuerySelector("table#marketDataList");
                var dataTable = mainTable.ChildNodes[2];

                var rows = dataTable.ChildNodes.Select(c => c as IHtmlTableRowElement).Where(c => c != null).ToList();

                foreach (var row in rows)
                {
                    var fields = row.Children.ToList();
                    var price = MapToPrice(fields, sourceType);
                    if (price != null)
                    {
                        retVal.Add(price);
                    }
                }
            }

            return retVal;
        }

        public static Uri GetUrl(SourceType sourceType, DateTime? date)
        {
            string url = string.Empty;
            switch (sourceType)
            {
                case SourceType.Blue:
                    url = "https://mfd.ru/marketdata/?id=5&mode=1";
                    break;
                case SourceType.MosBirja:
                    url = "https://mfd.ru/marketdata/?id=5&mode=3&group=16";
                    break;
                default:
                    break;
            }


            if (date.HasValue)
            {
                url += $"&selectedDate={date.Value.ToString("dd.MM.yyyy")}";
                //https://mfd.ru/marketdata/?id=5&mode=1&selectedDate=03.01.2023

            }

            return new Uri(url);
        }

        static Price? MapToPrice(List<IElement> fields, SourceType sourceType)
        {
            switch (sourceType)
            {
                case SourceType.Blue:
                    return MapToPriceBlue(fields);
                case SourceType.MosBirja:
                    return MapToPriceMosBirja(fields);
                default:
                    return null;
            }
        }

        private static string[] dateFormats = { "dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy", "HH:mm:ss" };

        private static Price? MapToPriceMosBirja(List<IElement> fields)
        {
            if (fields[2].ChildElementCount > 0)
            {
                return null;
            }

            return new Price(
                    fields[0].Children[1].InnerHtml, //string Ticker - 0 in
                    "D",   //string TimeFrame - D,
                    DateTime.ParseExact(fields[1].InnerHtml, dateFormats, CultureInfo.InvariantCulture).Date,   //DateTime Date - 1 format, 13.01.2023 18:45:05
                    0,//int ? Time - "0",
                    Utils.GetDouble(fields[6].InnerHtml),//double ? Open - 6,
                    Utils.GetDouble(fields[8].InnerHtml),//double ? High - 8,
                    Utils.GetDouble(fields[7].InnerHtml),//double ? Low - 7,
                    Utils.GetDouble(fields[2].InnerHtml),//double ? Close - 2,
                    Utils.GetDouble(fields[10].InnerHtml),//double ? Volume - 10,
                    0//int ? OpenInterest - "0"
                );
        }

        private static Price? MapToPriceBlue(List<IElement> fields)
        {
            if (fields[2].ChildElementCount > 0)
            {
                return null;
            }

            return new Price(
                    fields[0].Children[1].InnerHtml, //string Ticker - 0 in
                    "D",   //string TimeFrame - D,
                    DateTime.ParseExact(fields[1].InnerHtml, dateFormats, CultureInfo.InvariantCulture).Date,   //DateTime Date - 1 format, 13.01.2023 18:45:05
                    0,//int ? Time - "0",
                    Utils.GetDouble(fields[6].InnerHtml),//double ? Open - 6,
                    Utils.GetDouble(fields[8].InnerHtml),//double ? High - 8,
                    Utils.GetDouble(fields[7].InnerHtml),//double ? Low - 7,
                    Utils.GetDouble(fields[2].InnerHtml),//double ? Close - 2,
                    Utils.GetDouble(fields[10].InnerHtml),//double ? Volume - 10,
                    0//int ? OpenInterest - "0"
                );
        }
    }
}
