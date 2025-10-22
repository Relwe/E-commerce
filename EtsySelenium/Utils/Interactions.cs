using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace BestBuy.Utils
{
    internal class Interactions
    {
        public static void SafeTypeNumber(IWebDriver driver, IWebElement input, string value, TimeSpan? timeout = null)
        {
            timeout ??= TimeSpan.FromSeconds(60);
            var wait = new WebDriverWait(driver, timeout.Value);

            // 1) Scroll element into center (Firefox prefers this)
            ScrollIntoViewCenter(driver, input);

            // 2) Wait until it’s truly interactable (in viewport, visible, enabled, not covered)
            wait.Until(_ => IsInteractable(driver, input));

            // 3) Move mouse to force final layout/scroll in FF
            new Actions(driver).MoveToElement(input, 1, 1).Perform();

            // Small stability wait: ensure scrolling finished
            wait.Until(_ => (bool)((IJavaScriptExecutor)driver).ExecuteScript(@"
            const el = arguments[0];
            const r = el.getBoundingClientRect();
            const inViewport = r.top >= 0 && r.left >= 0 &&
                               r.bottom <= (window.innerHeight || document.documentElement.clientHeight) &&
                               r.right  <= (window.innerWidth  || document.documentElement.clientWidth);
            return inViewport;
        ", input));

            // 4) Try the normal path
            try
            {
                input.Click();
                input.Clear();
                input.SendKeys(value);
            }
            catch (ElementNotInteractableException)
            {
                // 5) Fallback: set value via JS and dispatch input/change (works with React/Vue/etc.)
                ((IJavaScriptExecutor)driver).ExecuteScript(@"
                const el = arguments[0], val = arguments[1];
                el.value = val;
                el.dispatchEvent(new Event('input', { bubbles: true }));
                el.dispatchEvent(new Event('change', { bubbles: true }));
            ", input, value);
            }
        }

        public static void ScrollIntoViewCenter(IWebDriver driver, IWebElement el)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript(@"
            const el = arguments[0];
            try { el.scrollIntoView({block:'center', inline:'nearest', behavior:'instant'}); }
            catch(e){ el.scrollIntoView(true); }
            // If a sticky header overlaps, adjust a bit upward
            const r = el.getBoundingClientRect();
            if (r.top < 80) window.scrollBy(0, r.top - 80);
        ", el);
        }

        public static bool IsInteractable(IWebDriver driver, IWebElement el)
        {
            try
            {
                var js = (IJavaScriptExecutor)driver;
                return (bool)js.ExecuteScript(@"
                const el = arguments[0];
                if (!el || !el.isConnected) return false;
                const style = getComputedStyle(el);
                if (style.visibility === 'hidden' || style.display === 'none') return false;
                if (el.disabled) return false;

                const r = el.getBoundingClientRect();
                const inViewport = r.width > 0 && r.height > 0 &&
                                   r.bottom > 0 && r.right > 0 &&
                                   r.top < (window.innerHeight || document.documentElement.clientHeight) &&
                                   r.left < (window.innerWidth  || document.documentElement.clientWidth);

                if (!inViewport) return false;

                // Check if something is covering the center point
                const x = Math.floor(r.left + r.width / 2);
                const y = Math.floor(r.top + r.height / 2);
                const elemAtPoint = document.elementFromPoint(x, y);
                return el === elemAtPoint || el.contains(elemAtPoint);
            ", el);
            }
            catch
            {
                return false;
            }
        }
    }
}
