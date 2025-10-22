using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace BestBuy.Pages
{
    internal class ResultsPage : BasePage
    {
        public ResultsPage(IWebDriver driver) : base(driver) { }

        public IWebElement MinPrice =>
            FindSmart(By.CssSelector("[aria-label=\"Minimum price\"]"));

        public IWebElement MaxPrice =>
            FindSmart(By.CssSelector("[aria-label=\"Maximum price\"]"));

        public IWebElement SetBtn =>
            FindSmart(By.CssSelector("button.current-price-facet-set-button"));

        public ResultsPage SetMinMaxPrice(string min, string max)
        {
            Utils.Waits.IsDomStable(driver);
            var sidebarBy = By.CssSelector("[class=\"sidebar-container\"]");
            var containerElement = driver.FindElement(sidebarBy);
            var js = (IJavaScriptExecutor)driver;
            js.ExecuteScript(@"
                const container = arguments[0];
                const target = arguments[1];
                container.scrollTop = target.offsetTop - container.offsetTop - (container.clientHeight / 2);
                ", containerElement, MinPrice);
            MinPrice.Clear();
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", MinPrice);

            Utils.Interactions.SafeTypeNumber(driver, MinPrice, min);
            MaxPrice.Clear();
            Utils.Interactions.SafeTypeNumber(driver, MaxPrice, max);
            SetBtn.Click();
            return this;
        }

        // ---------- Product List ----------

        private IWebElement GoToCartBtn => FindSmart(
            By.CssSelector("[title='Cart']"),
            By.CssSelector("[data-test='cart-icon'], a[href*='cart']")
        );

        public IReadOnlyList<IWebElement> Products => FindAllSmart(
            By.CssSelector("ul.product-grid-view-container li"),
            By.CssSelector("[data-testid='product-grid'] li"),
            By.CssSelector("ul[data-testid='search-results'] li")
        );

        public ResultsPage AddToCart(int index, out decimal price)
        {
            if (Products.Count == 0)
                throw new NoSuchElementException("No products found on the results page.");

            if (index < 0 || index >= Products.Count)
                throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range. Products count: {Products.Count}");

            var product = Products[index];

            // --- Price extraction ---
            var priceEl = wait.Until(_ =>
                FindWithinSmart(product,
                    By.CssSelector("[data-testid='price-block-customer-price'] span"),
                    By.CssSelector("[data-test='pricing-price'] span"),
                    By.CssSelector("[data-testid='localized-price']"),
                    By.CssSelector(".price, .product-price, [data-price]")
                ) is IWebElement el && el.Displayed ? el : null);

            var raw = priceEl.GetAttribute("textContent")?.Trim();
            if (string.IsNullOrEmpty(raw)) raw = priceEl.Text?.Trim();
            price = Utils.Price.ParsePrice(raw ?? string.Empty);
            Console.WriteLine($"Price: {price}");

            // --- Add to cart ---
            var addToCartBtn = wait.Until(_ =>
                FindWithinSmart(product,
                    By.ClassName("add-to-cart"),
                    By.CssSelector("[data-test='add-to-cart-button']"),
                    By.CssSelector("button.add-to-cart"),
                    By.CssSelector("button[type='button'][aria-label*='Add to cart']")
                ) is IWebElement el && el.Displayed && el.Enabled ? el : null);

            addToCartBtn.Click();

            // --- Handle navigation or modal ---
            var result = wait.Until(drv =>
            {
                if (drv.Url.Contains("/cart", StringComparison.OrdinalIgnoreCase))
                    return "Navigated";

                var modal = drv.FindElements(By.CssSelector("div[data-testid='drawer'], [role='dialog'], [data-test='add-to-cart-drawer']"))
                               .FirstOrDefault();
                if (modal != null && modal.Displayed)
                    return "ModalOpened";

                return null;
            });

            if (result == "Navigated")
            {
                driver.Navigate().Back();
            }
            else
            {
                actions.SendKeys(Keys.Escape).Perform();
            }

            wait.Until(drv => drv.FindElement(By.CssSelector("[class='sidebar-item-label']")).Displayed);
            return this;
        }

        public CartPage GoToCart()
        {
            GoToCartBtn.Click();
            return new CartPage(driver);
        }

        public string[] GetProductUrls(int limit)
        {
            limit = Math.Min(limit, Products.Count);
            var result = new string[limit];
            for (int i = 0; i < limit; i++)
            {
                var link = wait.Until(drv =>
                {
                    try
                    {
                        var a = Products[i].FindElement(By.TagName("a"));
                        return a.Displayed && a.Enabled ? a : null;
                    }
                    catch (NoSuchElementException)
                    {
                        return null;
                    }
                });
                result[i] = link.GetAttribute("href");
            }
            return result;
        }

    }
}
