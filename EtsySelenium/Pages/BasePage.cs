using OpenQA.Selenium;
using OpenQA.Selenium.DevTools.V138.Network;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace BestBuy.Pages
{
    internal class BasePage
    {
        protected readonly IWebDriver driver;
        protected readonly WebDriverWait wait;
        protected readonly Actions actions;
        public BasePage(IWebDriver driver)
        {
            this.driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
            actions = new Actions(driver);
        }
        
        protected IWebElement FindSmart(params By[] bys)
        {
            //var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            DismissPopupIfVisible();
            return wait.Until(drv =>
            {
                foreach (var by in bys)
                {
                    try
                    {
                        var element = drv.FindElement(by);

                        if (element.Displayed && element.Enabled)
                        {
                            Console.WriteLine($"Found using locator: {by}");
                            return element;
                        }
                    }
                    catch 
                    {
                        // try next
                    }
                }

                // returning null tells WebDriverWait to keep waiting
                return null;
            });
        }

        // Like FindSmart but scoped to a root element (product card, etc.)
        protected IWebElement? FindWithinSmart(IWebElement root, params By[] bys)
        {
            DismissPopupIfVisible();
            foreach (var by in bys)
            {
                try
                {
                    var el = root.FindElement(by);
                    if (el != null) return el;
                }
                catch (NoSuchElementException) { /* try next */ }
                catch (StaleElementReferenceException) { /* try next */ }
            }
            return null;
        }

        protected IReadOnlyList<IWebElement> FindAllSmart(params By[] bys)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            return wait.Until(drv =>
            {
                foreach (var by in bys)
                {
                    var list = drv.FindElements(by);
                    if (list.Count > 0)
                        return (IReadOnlyList<IWebElement>)list;
                }
                return null;
            })!;
        }

        public void DismissPopupIfVisible()
        {
            try
            {
                var popups = driver.FindElements(By.Id("survey_invite_no")); // your popup's button ID
                if (popups.Count > 0 && popups[0].Displayed)
                    popups[0].Click();
            }
            catch
            {
                // ignore — don't fail the test if popup not found or not clickable
            }
        }
    }
}