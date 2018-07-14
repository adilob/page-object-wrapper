using PageObjectWrapper.AutomationUI.Extensions;
using PageObjectWrapper.AutomationUI.Interfaces;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageObjectWrapper.AutomationUI
{
    public class PageObjectWrapper<T>
        where T : IAutomationPageObject
    {
        public PageObjectBase<T> PageBase { get; set; }

        protected IWebElement SelectedElement { get; set; }
        private By LastSelector { get; set; }

        public PageObjectWrapper(PageObjectBase<T> pageBase)
        {
            PageBase = pageBase;
        }

        public virtual PageObjectWrapper<T> SelectElement(By selector, bool throwExceptionIfNotFound = true)
        {
            SelectedElement = null;
            var element = PageBase.WebDriver.TryFindElement(selector, TimeSpan.FromSeconds(5));

            if (element != null)
            {
                SelectedElement = element;
            }
            else
            {
                if (throwExceptionIfNotFound)
                    throw new Exception(string.Format("By *** {0} *** object does not bind to Web element on page object!", selector != null ? selector.ToString() : "NULL"));
            }

            LastSelector = selector;
            return this;
        }

        public virtual PageObjectWrapper<T> SelectByText(string textToSelect)
        {
            if (SelectedElement == null)
                return this;

            var validator = PageBase.GetDefaultValidator();
            if (validator == null)
            {
                SelectedElement.SelectByText(textToSelect);
            }
            else if (validator != null && !validator.ExceptionFields.Any(selector => selector.Equals(LastSelector)))
            {
                SelectedElement.SelectByText(textToSelect);
            }

            return this;
        }

        public virtual PageObjectWrapper<T> SelectByIndex(int index)
        {
            if (SelectedElement == null)
                return this;

            var validator = PageBase.GetDefaultValidator();
            if (validator == null)
            {
                SelectedElement.SelectByIndex(index);
            }
            else if (validator != null && !validator.ExceptionFields.Any(selector => selector.Equals(LastSelector)))
            {
                SelectedElement.SelectByIndex(index);
            }

            return this;
        }

        public virtual PageObjectWrapper<T> IsEnabled(out bool result)
        {
            result = false;
            if (SelectedElement == null)
                return this;

            var validator = PageBase.GetDefaultValidator();
            if (validator == null)
            {
                result = SelectedElement.IsEnabled();
            }
            else if (validator != null && !validator.ExceptionFields.Any(selector => selector.Equals(LastSelector)))
            {
                result = SelectedElement.IsEnabled();
            }

            return this;
        }

        public virtual PageObjectWrapper<T> GetSelectedOptionText(out string result)
        {
            result = string.Empty;
            if (SelectedElement == null)
                return this;

            var validator = PageBase.GetDefaultValidator();
            if (validator == null)
            {
                result = SelectedElement.GetSelectedOptionText();
            }
            else if (validator != null && !validator.ExceptionFields.Any(selector => selector.Equals(LastSelector)))
            {
                result = SelectedElement.GetSelectedOptionText();
            }

            return this;
        }

        public virtual PageObjectWrapper<T> GetSelectOptionsCount(out int? result)
        {
            result = null;
            if (SelectedElement == null)
                return this;

            var validator = PageBase.GetDefaultValidator();
            if (validator == null)
            {
                result = SelectedElement.GetSelectOptionsCount();
            }
            else if (validator != null && !validator.ExceptionFields.Any(selector => selector.Equals(LastSelector)))
            {
                result = SelectedElement.GetSelectOptionsCount();
            }

            return this;
        }

        public virtual PageObjectWrapper<T> GetSelectOptions(out List<string> result)
        {
            result = null;
            if (SelectedElement == null)
                return this;

            var validator = PageBase.GetDefaultValidator();
            if (validator == null)
            {
                result = SelectedElement.AsSelectable().Options.Select(x => x.Text).ToList();
            }
            else if (validator != null && !validator.ExceptionFields.Any(selector => selector.Equals(LastSelector)))
            {
                result = SelectedElement.AsSelectable().Options.Select(x => x.Text).ToList();
            }

            return this;
        }

        public virtual PageObjectWrapper<T> Click()
        {
            if (SelectedElement == null)
                return this;

            var validator = PageBase.GetDefaultValidator();
            if (validator == null)
            {
                SelectedElement.Click();
            }
            else if (validator != null && !validator.ExceptionFields.Any(selector => selector.Equals(LastSelector)))
            {
                SelectedElement.Click();
            }

            return this;
        }

        public virtual PageObjectWrapper<T> IsSelectable(out bool isSelectable)
        {
            isSelectable = false;
            if (SelectedElement == null)
                return this;

            var validator = PageBase.GetDefaultValidator();
            if (validator == null)
            {
                isSelectable = SelectedElement.IsSelectable();
            }
            else if (validator != null && !validator.ExceptionFields.Any(selector => selector.Equals(LastSelector)))
            {
                isSelectable = SelectedElement.IsSelectable();
            }

            return this;
        }

        public virtual PageObjectWrapper<T> GetText(out string text)
        {
            text = string.Empty;
            if (SelectedElement == null)
                return this;

            var validator = PageBase.GetDefaultValidator();
            if (validator == null)
            {
                text = SelectedElement.Text;
            }
            else if (validator != null && !validator.ExceptionFields.Any(selector => selector.Equals(LastSelector)))
            {
                text = SelectedElement.Text;
            }

            return this;
        }

        public virtual PageObjectWrapper<T> ClearElement()
        {
            if (SelectedElement == null)
                return this;

            SelectedElement.Clear();
            return this;
        }

        public virtual PageObjectWrapper<T> SetText(string newText, bool clearElement = false)
        {
            if (SelectedElement == null)
                return this;

            var validator = PageBase.GetDefaultValidator();
            if (validator == null)
            {
                if (clearElement)
                    ClearElement();

                SelectedElement.SendKeys(newText);
            }
            else if (validator != null && !validator.ExceptionFields.Any(selector => selector.Equals(LastSelector)))
            {
                if (clearElement)
                    ClearElement();

                SelectedElement.SendKeys(newText);
            }

            return this;
        }

        public virtual PageObjectWrapper<T> ScrollIntoView()
        {
            if (SelectedElement == null)
                return this;

            var validator = PageBase.GetDefaultValidator();
            if (validator == null)
            {
                SelectedElement.ScrollIntoView(PageBase.WebDriver);
            }
            else if (validator != null && !validator.ExceptionFields.Any(selector => selector.Equals(LastSelector)))
            {
                SelectedElement.ScrollIntoView(PageBase.WebDriver);
            }

            return this;
        }

        public virtual PageObjectWrapper<T> TryGetAttribute(string attributeName, out string attributeValue, int tryCount = 3, int delay = 200, bool throwException = false)
        {
            attributeValue = null;
            if (SelectedElement == null)
                return this;

            var validator = PageBase.GetDefaultValidator();
            if (validator == null)
            {
                attributeValue = SelectedElement.TryGetAttribute(attributeName, tryCount, delay, throwException);
            }
            else if (validator != null && !validator.ExceptionFields.Any(selector => selector.Equals(LastSelector)))
            {
                attributeValue = SelectedElement.TryGetAttribute(attributeName, tryCount, delay, throwException);
            }

            return this;
        }

        public virtual PageObjectWrapper<T> IsDisplayed(out bool result)
        {
            result = false;
            if (SelectedElement == null)
                return this;

            var validator = PageBase.GetDefaultValidator();
            if (validator == null)
            {
                result = SelectedElement.IsDisplayed();
            }
            else if (validator != null && !validator.ExceptionFields.Any(selector => selector.Equals(LastSelector)))
            {
                result = SelectedElement.IsDisplayed();
            }

            return this;
        }
    }
}
