using PageObjectWrapper.AutomationUI.Extensions;
using PageObjectWrapper.AutomationUI.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PageObjectWrapper.AutomationUI
{
    /// <summary>
    /// Abstract page object to provide common functionality to specific implementations.
    /// </summary>
    /// <typeparam name="T">The interface type that defines the contract of the page object.</typeparam>
    public abstract class PageObjectBase<T> : IAutomationPageObject, IConvertible, IDisposable
        where T : IAutomationPageObject
    {
        #region Properties
        protected string CurrentWindowHandler = string.Empty;
        protected IReadOnlyCollection<string> OriginalHandles = null;
        protected PageObjectValidationBase<T> _validator = null;

        /// <summary>
        /// Provides the fluent intefaces of all operations on the <see cref="IWebElement"/> elements from page.
        /// </summary>
        protected PageObjectWrapper<T> Wrapper { get; set; }

        public IWebDriver WebDriver { get; set; }
        #endregion

        #region Constructor
        public PageObjectBase(IWebDriver webDriver)
        {
            WebDriver = webDriver;
            Wrapper = new PageObjectWrapper<T>(this);

            CurrentWindowHandler = WebDriver.CurrentWindowHandle;
            OriginalHandles = WebDriver.WindowHandles;
        }
        #endregion

        /// <summary>
        /// Selects an element from page to execute some operation on the wrapper object.
        /// </summary>
        /// <param name="func">The selector of the element on page.</param>
        /// <returns>A <see cref="PageObjectWrapper{T}"/> that contains all operations related to the element.</returns>
        public PageObjectWrapper<T> On(Func<T, By> func, bool throwExceptionIfNotFound = true)
        {
            T obj = (T)Convert.ChangeType(this, typeof(T));
            return Wrapper.SelectElement(func(obj), throwExceptionIfNotFound);
        }

        /// <summary>
        /// Gets a <see cref="IWebElement"/> element from page object.
        /// </summary>
        /// <param name="func">The selector of the element on page.</param>
        /// <param name="timeout">The timeout to wait load the element. If NULL, then the default timeout is 5 seconds.</param>
        /// <returns>A <see cref="IWebElement"/> element from page. Otherwise NULL.</returns>
        public IWebElement GetElement(Func<T, By> func, TimeSpan? timeout = null)
        {
            if (!timeout.HasValue)
                timeout = TimeSpan.FromSeconds(5);

            T obj = (T)Convert.ChangeType(this, typeof(T));
            return WebDriver.TryFindElement(func(obj), timeout.Value);
        }

        public IWebElement GetElementByDescription(string description)
        {
            var props = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => Attribute.IsDefined(p, typeof(DescriptionAttribute))).ToList();
            var property = props.FirstOrDefault(p =>
            {
                var descAttrib = (DescriptionAttribute)Attribute.GetCustomAttribute(p, typeof(DescriptionAttribute));
                return (descAttrib != null && descAttrib.Description.Equals(description));
            });

            if (property == null) return null;
            By locator = property.GetValue(this) as By;

            if (locator == null) return null;
            return WebDriver.TryFindElement(locator, TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Gets a <see cref="IReadOnlyCollection{T}"/> collection of elements from page.
        /// </summary>
        /// <param name="func">The selector of the elements from page.</param>
        /// <param name="timeout">The timeout to wait load the element. If NULL, then the default timeout is 5 seconds.</param>
        /// <returns>A <see cref="IReadOnlyCollection{T}"/> collection with all elements found on page. Otherwise, NULL.</returns>
        public IReadOnlyCollection<IWebElement> GetElements(Func<T, By> func, TimeSpan? timeout = null)
        {
            if (!timeout.HasValue)
                timeout = TimeSpan.FromSeconds(5);

            T obj = (T)Convert.ChangeType(this, typeof(T));
            return WebDriver.TryFindElements(func(obj), timeout.Value);
        }

        /// <summary>
        /// Gets the default validator object to the page object
        /// </summary>
        public virtual PageObjectValidationBase<T> GetDefaultValidator(bool throwExceptionWhenNull = false)
        {
            return null;
        }

        /// <summary>
        /// Switches the <see cref="IWebDriver"/> to the first popup the browser is presenting.
        /// </summary>
        /// <param name="elementToClick">The element to click and open the popup window.</param>
        /// <param name="timeout">The amount of time to wait for the popup to load.</param>
        /// <returns>An <see cref="IDisposable"/> page object.</returns>
        public IDisposable SwitchToPopup(Func<T, By> elementToClick, TimeSpan? timeout = null)
        {
            GetElement(elementToClick).Click();

            if (timeout == null)
                timeout = TimeSpan.FromSeconds(5);

            WebDriverWait wait = new WebDriverWait(WebDriver, timeout.Value);
            string popupHander = wait.Until(d => d.WindowHandles.Last());

            if (!string.IsNullOrEmpty(popupHander))
                WebDriver.SwitchTo().Window(popupHander);

            return this;
        }

        public void SwitchToFrame(string frameName)
        {
            WebDriver.SwitchTo().DefaultContent();
            WebDriver.SwitchTo().Frame(frameName);
        }

        public void SwitchToFrame(int frameIndex)
        {
            WebDriver.SwitchTo().DefaultContent();
            WebDriver.SwitchTo().Frame(frameIndex);
        }

        public void SwitchToFrame(IWebElement frameElement)
        {
            WebDriver.SwitchTo().DefaultContent();
            WebDriver.SwitchTo().Frame(frameElement);
        }

        public void SwitchBackToDefaultContent()
        {
            WebDriver.SwitchTo().DefaultContent();
        }

        public void WaitUntil<T1>(Func<IWebDriver, T1> condition, TimeSpan? timeout = null)
        {
            if (!timeout.HasValue)
                timeout = TimeSpan.FromSeconds(10);

            WebDriverWait wait = new WebDriverWait(WebDriver, timeout.Value);
            wait.Until(driver => condition(driver));
        }

        /// <summary>
        /// Switches the <see cref="IWebDriver"/> back to the main window on the browser.
        /// </summary>
        public void SwitchBackFromPopup()
        {
            if (!string.IsNullOrEmpty(CurrentWindowHandler))
                WebDriver.SwitchTo().Window(CurrentWindowHandler);
        }

        public virtual bool HasErrorMsg(string fieldLabel)
        {
            var xpathMsgError = string.Format("//*[text() = '{0}']/following::td[position()=1]//*[@class = 'errorMsg']", fieldLabel);
            var element = WebDriver.TryFindElement(By.XPath(xpathMsgError), TimeSpan.FromSeconds(5));
            return element != null && element.Displayed;
        }

        #region IConvertible Members
        public TypeCode GetTypeCode()
        {
            return TypeCode.Object;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            return false;
        }

        public char ToChar(IFormatProvider provider)
        {
            return char.MinValue;
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            return sbyte.MinValue;
        }

        public byte ToByte(IFormatProvider provider)
        {
            return Byte.MinValue;
        }

        public short ToInt16(IFormatProvider provider)
        {
            return Int16.MinValue;
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            return UInt16.MinValue;
        }

        public int ToInt32(IFormatProvider provider)
        {
            return Int32.MinValue;
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            return UInt32.MinValue;
        }

        public long ToInt64(IFormatProvider provider)
        {
            return Int64.MinValue;
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            return UInt64.MinValue;
        }

        public float ToSingle(IFormatProvider provider)
        {
            return Single.MinValue;
        }

        public double ToDouble(IFormatProvider provider)
        {
            return double.MinValue;
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            return decimal.MinValue;
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            return DateTime.MinValue;
        }

        public string ToString(IFormatProvider provider)
        {
            return ToString();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            if (typeof(T).IsAssignableFrom(conversionType))
                return this as IAutomationPageObject;

            if (typeof(IAutomationPageObject).IsAssignableFrom(typeof(T)))
                return this as IAutomationPageObject;

            return null;
        }

        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            SwitchBackFromPopup();
        }
        #endregion
    }
}
