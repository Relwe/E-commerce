// BaseTest.cs
using System;
using System.IO;
using System.Threading;
using AventStack.ExtentReports;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using static BestBuy.Utils.Screenshots;
using static BestBuy.Drivers.WebDriverFactory;
using BestBuy.Drivers;
using BestBuy.Infrastructure.Reporting;
[assembly: LevelOfParallelism(4)]

namespace BestBuy.Infrastructure
{
    [Parallelizable(ParallelScope.Fixtures)] // safe for parallel suites
    public abstract class BaseTest
    {
        // Thread-safe holders for parallel execution
        private static readonly AsyncLocal<IWebDriver> _driver = new();
        private static readonly AsyncLocal<ExtentTest> _test = new();

        protected IWebDriver Driver => _driver.Value;
        protected ExtentTest Log => _test.Value;

        private static ExtentReports _extent;
        private readonly BrowserType _browser;

        protected BaseTest(BrowserType browser)
        {
            _browser = browser;
        }

        [OneTimeSetUp]
        public void OneTimeSetUp_Base()
        {
            _extent = ReportManager.GetReporter();
        }

        [SetUp]
        public void SetUp_Base()
        {
            //var gridUrl = GetGridUrl();
            _driver.Value = Create(_browser);

            // 2) Start an Extent test node
            var testName = TestContext.CurrentContext.Test.Name;
            var suiteName = TestContext.CurrentContext.Test.ClassName;
            var parent = _extent.CreateTest(suiteName);
            _test.Value = parent.CreateNode(testName);

            // Optional tags/categories via NUnit attributes
            foreach (var cat in TestContext.CurrentContext.Test.Properties["Category"])
                Log.AssignCategory(cat.ToString());

            Log.Info($"Starting test: {testName}");
        }

        [TearDown]
        public void TearDown_Base()
        {
            try
            {
                var status = TestContext.CurrentContext.Result.Outcome.Status;
                var error = TestContext.CurrentContext.Result.Message;

                switch (status)
                {
                    case NUnit.Framework.Interfaces.TestStatus.Passed:
                        Log.Pass("✅ Test passed");
                        break;

                    case NUnit.Framework.Interfaces.TestStatus.Skipped:
                        Log.Skip("⏭️ Test skipped");
                        break;

                    default:
                        // On failure: take screenshot + attach
                        var (abs, rel) = SaveScreenshotToReportFolder($"FAIL_{Sanitize(TestContext.CurrentContext.Test.Name)}");
                        if (!string.IsNullOrEmpty(rel))
                            Log.Fail($"❌ Test failed: {error}").AddScreenCaptureFromPath(rel);
                        else
                            Log.Fail($"❌ Test failed: {error}");
                        break;
                }
            }
            finally
            {
                // Always quit driver
                Driver?.Quit();
                _driver.Value = null;
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown_Base()
        {
            _extent.Flush(); // writes the HTML report
        }

        protected void TakeAndAttachScreenshot(string description)
        {
            var (abs, rel) = SaveScreenshotToReportFolder(description);
            if (!string.IsNullOrEmpty(rel))
                Log.Info(description).AddScreenCaptureFromPath(rel);
            else
                Log.Warning($"Failed to take screenshot for: {description}");
        }


        protected (string absolute, string relative) SaveScreenshotToReportFolder(string fileBase)
        {
            try
            {
                var shotsDir = Path.Combine(ReportManager.ReportDirectory, "screenshots");
                Directory.CreateDirectory(shotsDir);

                var file = $"{fileBase}_{DateTime.Now:yyyyMMdd_HHmmss_fff}.png";
                var absolute = Path.Combine(shotsDir, file);

                var ss = ((ITakesScreenshot)Driver).GetScreenshot();
                File.WriteAllBytes(absolute, ss.AsByteArray);

                // compute relative path to report
                var relative = Path.GetRelativePath(ReportManager.ReportDirectory, absolute)
                                   .Replace('\\', '/'); // required for HTML
                return (absolute, relative);
            }
            catch (Exception ex)
            {
                Log.Warning($"Could not capture screenshot: {ex.Message}");
                return (string.Empty, string.Empty);
            }
        }


    }
}
