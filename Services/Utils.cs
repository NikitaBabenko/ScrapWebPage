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
    }
}
