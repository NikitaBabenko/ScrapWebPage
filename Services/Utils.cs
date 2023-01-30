using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    internal static class Utils
    {
        public static double GetDouble(string source)
        {
            string toParse = string.Concat(source).Replace(" ", string.Empty);
            double.TryParse(toParse, CultureInfo.InvariantCulture, out double retVal);
            return retVal;
        }

        public static decimal GetDecimal(string source)
        {
            string toParse = string.Concat(source).Replace(" ", string.Empty);
            decimal.TryParse(toParse, CultureInfo.InvariantCulture, out decimal retVal);
            return retVal;
        }

        public static string GetProcents(double source) 
        {
            return string.Format("{0:0.##}", source) + "%";
        }
    }
}
