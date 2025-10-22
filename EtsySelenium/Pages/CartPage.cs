using OpenQA.Selenium;

namespace BestBuy.Pages
{
    internal class CartPage : BasePage
    {
        public CartPage(IWebDriver driver) : base(driver) { }

        IWebElement subtotalCell => FindSmart(By.CssSelector("tr.bold-last-child td"));

        public decimal GetSubtotal()
        {
            var subtotalText = subtotalCell.Text;
            return Utils.Price.ParsePrice(subtotalText);
        }
    }
}