// ReportManager.cs
using System.IO;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using NUnit.Framework;

namespace BestBuy.Infrastructure.Reporting
{
    public static class ReportManager
    {
        public static string? ReportDirectory { get; private set; }
        private static readonly object _lock = new();
        private static ExtentReports? _extent;

        public static ExtentReports GetReporter()
        {
            if (_extent != null) return _extent;
            lock (_lock)
            {
                if (_extent != null) return _extent;

                var reportsDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "TestResults", "Extent");
                Directory.CreateDirectory(reportsDir);
                ReportDirectory = reportsDir;

                ReportDirectory = reportsDir; // expose path

                var reportPath = Path.Combine(reportsDir, "index.html");
                var spark = new ExtentSparkReporter(reportPath);
                spark.Config.DocumentTitle = "Automation Test Report";
                spark.Config.ReportName = "UI Regression Suite";

                _extent = new ExtentReports();
                _extent.AttachReporter(spark);

                _extent.AddSystemInfo("OS", Environment.OSVersion.ToString());
                _extent.AddSystemInfo(".NET", Environment.Version.ToString());
                _extent.AddSystemInfo("Runner", "NUnit");
            }
            return _extent;
        }
    }
}
