using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace BestBuy.Utils
{
    internal class Waits
    {
        public static bool IsDomStable(IWebDriver driver)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            var oldCount = driver.FindElements(By.CssSelector("*")).Count;
            var stableSince = DateTime.Now;

            try
            {
                wait.Until(d =>
                {
                    var newCount = d.FindElements(By.CssSelector("*")).Count;

                    if (newCount == oldCount)
                    {
                        if ((DateTime.Now - stableSince).TotalSeconds > 1)
                            return true;
                    }
                    else
                    {
                        stableSince = DateTime.Now;
                        oldCount = newCount;
                    }
                    return false;
                });
                return true; // DOM stabilized
            }
            catch (WebDriverTimeoutException)
            {
                return false; // DOM didn't stabilize in time
            }
        }

    }
}
