using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace BestBuy.Pages
{
    internal class HomePage : BasePage
    {
        public IWebElement SearchField => FindSmart(By.Id("autocomplete-search-bar"), By.XPath("//*[@id=\"autocomplete-search-bar\"]"));
        public IWebElement SearchBtn => FindSmart(By.Id("autocomplete-search-button"), By.XPath("//*[@id=\"autocomplete-search-button\"]"));
        
        public HomePage(IWebDriver driver) : base(driver) { }
      

        public object Driver { get; }

        

        internal void GoTo()
        {
            driver.Navigate().GoToUrl("https://www.bestbuy.com/?intl=nosplash");
            Utils.Waits.IsDomStable(driver);
        }

        internal ResultsPage Search(string products)
        {
            SearchField.Clear();
            SearchField.SendKeys(products);
            SearchBtn.Click();
            return new ResultsPage(driver);
        }

        public string[] SearchItemsByNameUnderPrice(string query, int maxPrice, int limit)
        {
            var resultsPage = Search(query);
            Utils.Waits.IsDomStable(driver);
            resultsPage.SetMinMaxPrice("0", maxPrice.ToString());
            return resultsPage.GetProductUrls(limit);
        }


    }
}
