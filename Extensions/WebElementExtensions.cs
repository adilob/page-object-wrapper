using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageObjectWrapper.AutomationUI.Extensions
{
    public static class WebElementExtensions
    {
        public static bool IsEnabled(this IWebElement element)
        {
            if (element == null)
                return false;

            return element.Enabled;
        }

        public static bool IsDisplayed(this IWebElement element)
        {
            if (element == null)
                return false;

            return element.Displayed;
        }

        /// <summary>
        /// Gets whether the element is selectable or not
        /// </summary>
        /// <returns>TRUE if the element is selectable. Otherwise, FALSE</returns>
        public static bool IsSelectable(this IWebElement element, bool throwException = false)
        {
            if (element == null)
                return false;

            SelectElement selectElement = null;
            bool result = true;

            try
            {
                selectElement = new SelectElement(element);
                result = selectElement != null && selectElement.Options != null;
            }
            catch (Exception ex)
            {
                if (throwException)
                    throw ex;

                result = false;
            }

            return result;
        }

        /// <summary>
        /// Gets an instance of <see cref="SelectElement"/> object
        /// </summary>
        public static SelectElement AsSelectable(this IWebElement element)
        {
            if (element == null)
                return null;

            return new SelectElement(element);
        }

        /// <summary>
        /// If the element is selectable, gets the options count
        /// </summary>
        /// <returns>An integer value. Otherwise, NULL.</returns>
        public static int? GetSelectOptionsCount(this IWebElement element)
        {
            if (!element.IsSelectable())
                return null;

            return element.AsSelectable().Options.Count;
        }

        public static IEnumerable<string> GetSelectOptionsAsEnumerable(this IWebElement element)
        {
            if (!element.IsSelectable())
                return null;

            var options = element.AsSelectable().Options.Select(item => item.Text);
            return options;
        }

        /// <summary>
        /// If the element is selectable, selects the option's item by text
        /// </summary>
        /// <param name="text">The text to select</param>
        public static void SelectByText(this IWebElement element, string text)
        {
            if (!element.IsSelectable())
                return;

            element.AsSelectable().SelectByText(text);
        }

        public static void SelectByIndex(this IWebElement element, int index)
        {
            if (!element.IsSelectable())
                return;

            element.AsSelectable().SelectByIndex(index);
        }

        /// <summary>
        /// If the element is selectable, gets the selected option
        /// </summary>
        /// <returns>A <see cref="IWebElement"/> element that represents the selected option</returns>
        public static IWebElement GetSelectedOption(this IWebElement element)
        {
            if (!element.IsSelectable())
                return null;

            return element.AsSelectable().SelectedOption;
        }

        /// <summary>
        /// If the element is selectable, gets the seleted option's text
        /// </summary>
        public static string GetSelectedOptionText(this IWebElement element)
        {
            if (!element.IsSelectable())
                return null;

            if (element.GetSelectedOption() == null)
                return null;

            return element.GetSelectedOption().Text;
        }

        public static void ScrollIntoView(this IWebElement element, IWebDriver webDriver, int delay = 500)
        {
            if (element == null || webDriver == null)
                return;

            var jsExecutor = webDriver as IJavaScriptExecutor;

            if (jsExecutor != null)
            {
                jsExecutor.ExecuteScript("arguments[0].scrollIntoView(true);", element);
            }

            System.Threading.Thread.Sleep(delay);
        }

        public static void TryClick(this IWebElement element, int tryCount = 3, int delay = 1000, bool throwException = false)
        {
            if (element == null)
                return;

            var doClick = true;
            var clickcount = 0;

            while (doClick && clickcount <= tryCount)
            {
                try
                {
                    element.Click();
                    doClick = false;
                }
                catch (Exception ex)
                {
                    if (clickcount >= tryCount && throwException)
                        throw ex;
                }
                finally
                {
                    clickcount++;
                    System.Threading.Thread.Sleep(delay);
                }
            }
        }

        public static string TryGetAttribute(this IWebElement element, string attributeName, int tryCount = 3, int delay = 200, bool throwException = false)
        {
            if (element == null)
                return string.Empty;

            var getAttrib = true;
            var getAttribCount = 0;

            string result = string.Empty;

            while (getAttrib && getAttribCount <= tryCount)
            {
                try
                {
                    result = element.GetAttribute(attributeName);
                    break;
                }
                catch (Exception ex)
                {
                    if (getAttribCount >= tryCount && throwException)
                        throw ex;
                }
                finally
                {
                    getAttribCount++;
                    System.Threading.Thread.Sleep(delay);
                }
            }

            return result;
        }
    }
}
