using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PageObjectWrapper.AutomationUI.Extensions
{
    public static class WebDriverExtensions
    {
        /// <summary>
        /// Gets whether the element exists or not on the page object.
        /// </summary>
        /// <param name="webDriver">The webDriver being executed.</param>
        /// <param name="by">The condition for locate the element.</param>
        /// <param name="timeout">The amount of time to wait until the element is loaded.</param>
        /// <returns>TRUE if the element exists on the page object. Otherwise, FALSE.</returns>
        public static bool Exists(this IWebDriver webDriver, By by, TimeSpan timeout)
        {
            if (webDriver == null || by == null)
                return false;

            try
            {
                // TODO: implementar o timeout
                var collection = webDriver.FindElements(by);
                return collection != null && collection.Any();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets whether the element exists or not on the page object.
        /// </summary>
        /// <param name="webDriver">The webDriver being executed.</param>
        /// <param name="by">The condition for locate the element.</param>
        /// <returns>TRUE if the element exists on the page object. Otherwise, FALSE.</returns>
        public static bool Exists(this IWebDriver webDriver, By by)
        {
            if (webDriver == null || by == null)
                return false;

            var collection = webDriver.FindElements(by);
            return collection != null && collection.Any();
        }

        /// <summary>
        /// Tries to find the element on the page object
        /// </summary>
        /// <param name="webDriver">The webDriver being executed.</param>
        /// <param name="by">The condition for locate the element.</param>
        /// <param name="timeout">The amount of time to wait for loading the element.</param>
        /// <returns>Returns the <see cref="IWebElement"/> if it exists on the page object. Otherwise returns NULL.</returns>
        public static IWebElement TryFindElement(this IWebDriver webDriver, By by, TimeSpan timeout)
        {
            if (webDriver == null || by == null)
                return null;

            if (!webDriver.Exists(by, timeout))
                return null;

            return webDriver.FindElement(by);
        }

        public static IReadOnlyCollection<IWebElement> TryFindElements(this IWebDriver webDriver, By by, TimeSpan timeout)
        {
            if (webDriver == null || by == null)
                return null;

            if (!webDriver.Exists(by, timeout))
                return null;

            return webDriver.FindElements(by);
        }

        public static T WaitUntil<T>(this IWebDriver webDriver, Func<IWebDriver, T> condition, TimeSpan timeout)
        {
            var result = default(T);

            try
            {
                if (webDriver == null)
                    return default(T);

                var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(webDriver, timeout);
                var element = wait.Until(driver => condition(driver));

                if (element != null)
                    result = (T)element;

            } catch { }

            return result;
        }
    }
}
