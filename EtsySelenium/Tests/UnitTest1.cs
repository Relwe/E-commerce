using System.IO;
using BestBuy.Infrastructure;
using BestBuy.Pages;
using OpenQA.Selenium;
using SeleniumCsvDemo.Utils;
using static BestBuy.Drivers.WebDriverFactory;

namespace BestBuy.Tests
{
    [TestFixture(BrowserType.Chrome)]
    [TestFixture(BrowserType.Firefox)]
    public class Tests : BaseTest
    {
        public Tests(BrowserType browser) : base(browser) { }
        [Test]
        [TestCaseSource(typeof(TestDataCsv), nameof(TestDataCsv.SearchStrings))]
        public void Test1(string query, int maxPrice, int itemsCount)
        {
            //decimal sum = 0;
            HomePage homePage = new HomePage(Driver);
            Driver.Manage().Window.Maximize();
            homePage.GoTo();
            var results = homePage.SearchItemsByNameUnderPrice(query, maxPrice, itemsCount);
            AddItemsToCart(results);
            AssertCartTotalNotExceeds(maxPrice, itemsCount);
            
        }

        public void AddItemsToCart(string[] urls)
        {
            foreach (string url in urls)
            {
                Driver.Navigate().GoToUrl(url.ToString());
                
                var itemPage = new ItemPage(Driver, Log);
                itemPage.AddToCart();
                
                Driver.Navigate().Back();
            }
        }

        public void AssertCartTotalNotExceeds(int budgetPerItem, int itemCount)
        {
            Driver.Navigate().GoToUrl("https://www.bestbuy.com/cart");
            var cartPage = new CartPage(Driver);
            Assert.That(cartPage.GetSubtotal(), Is.LessThan(budgetPerItem * itemCount));
        }


    }


}
