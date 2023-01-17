using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Services
{
    public class ParseWebsiteServiceHtmlAgilityPack
    {
        public void Test()
        {
            using (WebClient client = new())
            {
                //string url = "https://mfd.ru/marketdata/?id=5&mode=1";
                string url = "https://mfd.ru/marketdata/?id=5&mode=3&group=16";
                string htmlCode = client.DownloadString(url);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(HttpUtility.HtmlDecode(htmlCode));

                //HtmlWeb
                //HtmlDocument doc = new HtmlDocument();
                //doc.LoadHtml(htmlCode);

                //var datePicker = doc.DocumentNode.SelectSingleNode("//span[contains(@class, 'mfd-header-endofday')]").SelectSingleNode("//input[contains(@class, 'mfd-input-date')]");
                var datePicker = doc.DocumentNode.SelectSingleNode("//input[contains(@class, 'mfd-input-date')]");
                string date = datePicker.Attributes["value"].Value;

                var tables = doc.GetElementbyId("marketDataList").SelectNodes("//tbody");
                var dataTable = tables[0];
                //var lines = dataTable.SelectNodes("//tr");
                //StringBuilder sb = new StringBuilder(dataTable.InnerHtml);
                var dsdfsdfs = dataTable.InnerHtml;
                //var linesads = dataTable.InnerHtml.Split(Environment.NewLine).Select(l => l.Trim()).Where(l => l.StartsWith("<tr><td>")).ToList();
                //var dsflines = sb.ToString().Split(Environment.NewLine).Select(l => l.Trim()).Where(l => l.StartsWith("<tr><td>")).ToList();


                // <tr><td><span class='mfdU'></span><a href="/marketdata/ticker/?id=56252" data-id="56252">Polymetal</a><td>13.01.2023 23:50:02<td>403<td><span class='mfd-u'>+15.7</span><td><span class='mfd-u'>+4.05%</span><td>387.3<td>388.4<td>385<td>404.6<td>398.3<td>2 511 542<td>1 000 307 335<td>45 832

                //WebUtility.H
                //HttpUtility.Ht

                int sdfdsf = 4;
            }
        }
    }
}
