////using System.Collections.Generic;
////using System.Globalization;
////using OpenQA.Selenium;

////namespace EtsySelenium
////{
////    internal class FilteredResultsPage : BasePage
////    {
////        public FilteredResultsPage(IWebDriver driver) : base(driver){ }

////        IWebElement GoToCartBtn => FindSmart(By.CssSelector("[title=\"Cart\"]"));
////        public List<IWebElement> Products =>
////            wait.Until(drv =>
////            {
////                var lists = drv.FindElements(By.CssSelector("ul.product-grid-view-container"));
////                if (lists.Count == 0) return null;

////                var items = lists.First().FindElements(By.TagName("li")).ToList();
////                return items.Count > 0 ? items : null;
////            });
////        public FilteredResultsPage AddToCart(int index, out decimal price)
////        {

////            var priceEl = wait.Until(drv =>
////            {
////                var el = Products[index].FindElements(
////                    By.CssSelector("[data-testid='price-block-customer-price'] span"))
////                    .FirstOrDefault();
////                return (el != null && el.Displayed) ? el : null;
////            });

////            var raw = priceEl.GetAttribute("textContent")?.Trim() ?? priceEl.Text.Trim();

////            price = Utils.ParsePrice(raw);
////            Console.WriteLine(price);
////            Products[index].FindElement(By.ClassName("add-to-cart")).Click();
////            var result = wait.Until(drv =>
////            {
////                // Option 1: new page loaded (URL changed or specific element appears)
////                if (drv.Url.Contains("https://www.bestbuy.com/cart"))
////                    return "Navigated";

////                // Option 2: modal appeared
////                var modal = drv.FindElements(By.CssSelector("div[data-testid=\"drawer\"]")).FirstOrDefault();
////                if (modal != null && modal.Displayed)
////                    return "ModalOpened";

////                return null;
////            });
////            if (result == "Navigated")
////                driver.Navigate().Back();
////            else
////                actions.SendKeys(Keys.Escape).Perform();
////            wait.Until(drv => drv.FindElement(By.CssSelector("[class=\"sidebar-item-label\"]")).Displayed);
////            return new FilteredResultsPage(this.driver);
////        }
////        public CartPage GoToCart()
////        {
////            GoToCartBtn.Click();
////            return new CartPage(this.driver);
////        }
////    }
////}






//using System;
//using System.Collections.Generic;
//using System.Linq;
//using OpenQA.Selenium;

//namespace EtsySelenium
//{
//    internal class FilteredResultsPage : BasePage
//    {
//        public FilteredResultsPage(IWebDriver driver) : base(driver) { }

//        // Cart button with a fallback
//        private IWebElement GoToCartBtn => FindSmart(
//            By.CssSelector("[title='Cart']"),
//            By.CssSelector("[data-test='cart-icon'], a[href*='cart']")
//        );

//        // Product tiles: return the first non-empty locator result
//        public IReadOnlyList<IWebElement> Products => FindAllSmart(
//            By.CssSelector("ul.product-grid-view-container li"),
//            By.CssSelector("[data-testid='product-grid'] li"),
//            By.CssSelector("ul[data-testid='search-results'] li")
//        );

//        public FilteredResultsPage AddToCart(int index, out decimal price)
//        {
//            if (Products.Count == 0)
//                throw new NoSuchElementException("No products found on the results page.");

//            if (index < 0 || index >= Products.Count)
//                throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range. Products count: {Products.Count}");

//            var product = Products[index];

//            // Price element within the specific product card (with fallbacks)
//            var priceEl = wait.Until(_ =>
//                FindWithinSmart(product,
//                    By.CssSelector("[data-testid='price-block-customer-price'] span"),
//                    By.CssSelector("[data-test='pricing-price'] span"),
//                    By.CssSelector("[data-testid='localized-price']"),
//                    By.CssSelector(".price, .product-price, [data-price]")
//                ) is IWebElement el && el.Displayed ? el : null);

//            var raw = priceEl.GetAttribute("textContent")?.Trim();
//            if (string.IsNullOrEmpty(raw)) raw = priceEl.Text?.Trim();

//            price = Utils.ParsePrice(raw ?? string.Empty);
//            Console.WriteLine(price);

//            // Add to Cart inside the same product card (with fallbacks)
//            var addToCartBtn = wait.Until(_ =>
//                FindWithinSmart(product,
//                    By.ClassName("add-to-cart"),
//                    By.CssSelector("[data-test='add-to-cart-button']"),
//                    By.CssSelector("button.add-to-cart"),
//                    By.CssSelector("button[type='button'][aria-label*='Add to cart']")
//                ) is IWebElement el && el.Displayed && el.Enabled ? el : null);

//            addToCartBtn.Click();

//            // Handle the two possible behaviors: navigation vs. modal
//            var result = wait.Until(drv =>
//            {
//                // Option 1: navigated to cart page
//                if (drv.Url.Contains("bestbuy.com/cart", StringComparison.OrdinalIgnoreCase) ||
//                    drv.Url.Contains("/cart", StringComparison.OrdinalIgnoreCase))
//                {
//                    return "Navigated";
//                }

//                // Option 2: modal/drawer appeared
//                var modal = drv.FindElements(By.CssSelector("div[data-testid='drawer'], [role='dialog'], [data-test='add-to-cart-drawer']"))
//                               .FirstOrDefault();
//                if (modal != null && modal.Displayed)
//                    return "ModalOpened";

//                return null;
//            });

//            if (result == "Navigated")
//            {
//                driver.Navigate().Back();
//            }
//            else // "ModalOpened"
//            {
//                actions.SendKeys(Keys.Escape).Perform();
//            }

//            // Wait for page to stabilize (same as your original signal)
//            wait.Until(drv => drv.FindElement(By.CssSelector("[class='sidebar-item-label']")).Displayed);

//            return new FilteredResultsPage(this.driver);
//        }

//        public CartPage GoToCart()
//        {
//            GoToCartBtn.Click();
//            return new CartPage(this.driver);
//        }

//        // ---------- helpers ----------

//        // Tries each locator and returns the first non-empty list
//        private IReadOnlyList<IWebElement> FindAllSmart(params By[] bys)
//        {
//            return wait.Until(drv =>
//            {
//                foreach (var by in bys)
//                {
//                    var list = drv.FindElements(by);
//                    if (list.Count > 0) return (IReadOnlyList<IWebElement>)list;
//                }
//                return null;
//            })!;
//        }

//        public string[] GetProductUrls(int limit)
//        {
//            var result = new string[limit];
//            for (int i = 0; i < limit; i++) 
//            {
//                var link = Products[i].FindElement(By.TagName("a"));
//                result[i] = link.GetAttribute("href");
                
//            }
//            return result;
//        }
//    }
//}
