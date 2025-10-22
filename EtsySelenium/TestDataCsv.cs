using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SeleniumCsvDemo.Utils
{
    public static class TestDataCsv
    {
        public static IEnumerable<TestCaseData> SearchStrings()
        {
            var path = Path.Combine(
                TestContext.CurrentContext.TestDirectory, "Data", "data.csv");

            foreach (var line in File.ReadLines(path).Skip(1)) // skip header
            {
                var parts = line.Split(',');
                var search = parts[0].Trim();
                var maxPrice = int.Parse(parts[1].Trim());
                var itemsCount = int.Parse(parts[2].Trim());

                yield return new TestCaseData(search, maxPrice, itemsCount)
                    .SetName($"Search_{search}_{maxPrice}_{itemsCount}");
            }
        }
    }

}

