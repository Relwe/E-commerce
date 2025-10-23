
using System.Linq.Expressions;
using AventStack.ExtentReports;
using OpenQA.Selenium;
using SeleniumExtras.WaitHelpers;
using static BestBuy.Utils.Waits;

namespace BestBuy.Pages
{
    internal class ItemPage : BasePage
    {
        private readonly ExtentTest _log;
        public ItemPage(IWebDriver driver, ExtentTest log) : base(driver) 
        {
            _log = log;
        }

        public IWebElement AddToCartBtn => FindSmart(By.CssSelector("[data-test-id=\"add-to-cart\"]"), By.XPath("//*[@id=\"a2c\"]/div/div/div/button"));

        internal void AddToCart()
        {
            IsDomStable(driver);
            var ss = ((ITakesScreenshot)driver).GetScreenshot();
            var base64 = ss.AsBase64EncodedString;
            var media = MediaEntityBuilder
                          .CreateScreenCaptureFromBase64String(base64, "Item page before AddToCart")
                          .Build();
            _log.Info("Visual checkpoint: item ready to add to cart", media);
            
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", AddToCartBtn);
            wait.Until(ExpectedConditions.ElementToBeClickable(AddToCartBtn));
            IsDomStable(driver);
            AddToCartBtn.Click();
            wait.Until(drv => { return AddToCartBtn.Text.Trim() == "Add to cart"; });

        }
    }
}