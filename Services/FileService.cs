using Services.Dto;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class FileService
    {
        private const string firstLine = "<TICKER>,<PER>,<DATE>,<TIME>,<OPEN>,<HIGH>,<LOW>,<CLOSE>,<VOL>,<OPENINT>";

        public List<string> GetFiles(string dirPath)
        {
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(dirPath));
            foreach (var dir in Directory.GetDirectories(dirPath))
            {
                files.AddRange(GetFiles(dir));
            }

            return files;
        }

        public long GetFileLength(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 1024 * 1024))
            {
                long lineCount = 0;
                byte[] buffer = new byte[1024 * 1024];
                int bytesRead;

                do
                {
                    bytesRead = fs.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < bytesRead; i++)
                        if (buffer[i] == '\n')
                            lineCount++;
                }
                while (bytesRead > 0);

                return lineCount;
            }
        }

        public IEnumerable<Price> GetPrices(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            {
                string line;
                bool isFirstLine = true;
                while ((line = reader.ReadLine()) != null)
                {
                    if(isFirstLine)
                    {
                        if(line != firstLine)
                        {
                            throw new Exception("Некорректный формат файла");
                        }
                        isFirstLine = false;
                        continue;
                    }

                    var retVal = MapToPrice(line);
                    //if(retVal != null)
                    //{
                    //    yield return retVal;
                    //}
                    yield return retVal;
                }
            }
        }

        private static Price MapToPrice(string source)
        {
            var s = source.Split(',');
            var timeFrame = s[1];
            if(timeFrame != "D")
            {
                return null;
            }
            int.TryParse(s[3], out int time);
            int.TryParse(s[9], out int openInt);
            var retVal = new Price(
                s[0],//<TICKER> //s[0].TrimStart('^').TrimStart('^')
                timeFrame,//<PER> TimeFrame D
                DateTime.ParseExact(s[2], "yyyyMMdd", CultureInfo.InvariantCulture).Date,//<DATE>
                time,//<TIME>
                Utils.GetDouble(s[4]), //<OPEN>
                Utils.GetDouble(s[5]), //<HIGH>
                Utils.GetDouble(s[6]), //<LOW>
                Utils.GetDouble(s[7]), //<CLOSE>
                Utils.GetDouble(s[8]), //<VOL>
                openInt //<OPENINT>
                //null, null, null, null, Enums.Signal.None
                );
            return retVal;
        }
    }
}
