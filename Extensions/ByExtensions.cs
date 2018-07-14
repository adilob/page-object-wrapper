using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageObjectWrapper.AutomationUI.Extensions
{
    public static class ByExtensions
    {
        public static string GetValue(this By by, IWebDriver webdriver)
        {
            if (by != null && webdriver != null)
            {
                var element = webdriver.TryFindElement(by, TimeSpan.FromSeconds(5));
                if (element != null)
                {
                    if (element.IsSelectable())
                        return element.GetSelectedOptionText();
                    else
                        return element.Text;
                }
            }

            return null;
        }
    }
}
