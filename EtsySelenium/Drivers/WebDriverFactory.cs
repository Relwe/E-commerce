using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.BiDi.Communication;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using NUnit.Framework;

namespace BestBuy.Drivers
{
    public static class WebDriverFactory
    {
        static bool headless = bool.TryParse(TestContext.Parameters.Get("Headless"), out var isHeadless) && isHeadless;

        public enum RunTarget { Local, Remote }
        public enum BrowserType { Chrome, Firefox }

        public static IWebDriver Create(BrowserType browser)
        {
            var target = GetRunTarget();                 // env RUN_TARGET=local|remote (default: remote)
            var grid = GetGridUrl();                   // env GRID_URL (default http://localhost:4444)

            return target == RunTarget.Local
                ? CreateLocal(browser)
                : CreateRemote(browser, grid);
        }

        private static RunTarget GetRunTarget()
        {
            var v = Environment.GetEnvironmentVariable("RUN_TARGET");
            return string.Equals(v, "local", StringComparison.OrdinalIgnoreCase)
                ? RunTarget.Local
                : RunTarget.Remote;
        }
        public static Uri GetGridUrl()
        {
            var fromEnv = Environment.GetEnvironmentVariable("GRID_URL");
            if (!string.IsNullOrWhiteSpace(fromEnv)) return new Uri(fromEnv);

            // Works when running tests on host
            return new Uri("http://localhost:4444");
        }

        private static BrowserType GetBrowser()
        {
            // 1) NUnit TestParameters (runsettings) win
            var fromParams = TestContext.Parameters.Get("Browser");
            if (!string.IsNullOrWhiteSpace(fromParams) &&
                Enum.TryParse<BrowserType>(fromParams, true, out var b1))
                return b1;

            // 2) env var BROWSER (GitHub Actions matrix)
            var fromEnv = Environment.GetEnvironmentVariable("BROWSER");
            if (!string.IsNullOrWhiteSpace(fromEnv) &&
                Enum.TryParse<BrowserType>(fromEnv, true, out var b2))
                return b2;

            // 3) default
            return BrowserType.Chrome;
        }

        public static IWebDriver Create() => Create(GetBrowser());

        // ---------- Local drivers (Selenium Manager will fetch binaries automatically) ----------
        private static IWebDriver CreateLocal(BrowserType browser)
        {
            return browser switch
            {
                BrowserType.Chrome => CreateLocalChrome(),
                BrowserType.Firefox => CreateLocalFirefox(),
                _ => throw new ArgumentOutOfRangeException(nameof(browser))
            };
        }

        private static IWebDriver CreateLocalChrome()
        {
            var svc = ChromeDriverService.CreateDefaultService();
            var opts = new ChromeOptions
            {
                AcceptInsecureCertificates = true
            };
            if (headless)
                opts.AddArgument("--headless=new");
            var drv = new ChromeDriver(svc, opts, TimeSpan.FromMinutes(3));
            drv.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60);
            drv.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(0);
            return drv;
        }

        private static IWebDriver CreateLocalFirefox()
        {
            var service = FirefoxDriverService.CreateDefaultService();
            var options = new FirefoxOptions
            {
                AcceptInsecureCertificates = true,
                PageLoadStrategy = PageLoadStrategy.Eager
            };
            if (headless)
                options.AddArgument("--headless");

            var driver = new FirefoxDriver(service, options, TimeSpan.FromMinutes(3));

            driver.Manage().Timeouts().PageLoad = TimeSpan.FromMinutes(2);
            driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(30);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;

            return driver;
        }
        private static IWebDriver CreateRemote(BrowserType browser, Uri gridUrl)
        {
            var cmdTimeout = TimeSpan.FromMinutes(3);

            return browser switch
            {
                BrowserType.Chrome => new RemoteWebDriver(
                    gridUrl,
                    new ChromeOptions
                    {
                        AcceptInsecureCertificates = true
                    }.ToCapabilities(),
                    cmdTimeout),

                BrowserType.Firefox => new RemoteWebDriver(
                    gridUrl,
                    new FirefoxOptions
                    {
                        AcceptInsecureCertificates = true
                    }.ToCapabilities(),
                    cmdTimeout),

                _ => throw new ArgumentOutOfRangeException(nameof(browser))
            };
        }
    }
}

