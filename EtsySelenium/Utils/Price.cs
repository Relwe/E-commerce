using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestBuy.Utils
{
    internal class Price
    {
        public static decimal ParsePrice(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                throw new ArgumentException("Price string cannot be null or empty.", nameof(raw));

            // Keep only digits, dot, or comma
            var numeric = new string(raw
                .Where(c => char.IsDigit(c) || c == '.' || c == ',')
                .ToArray());

            // Remove thousands separators (commas)
            numeric = numeric.Replace(",", "");

            // Parse to decimal
            return decimal.Parse(numeric, CultureInfo.InvariantCulture);
        }
    }
}
