using PageObjectWrapper.AutomationUI.Extensions;
using PageObjectWrapper.AutomationUI.Interfaces;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;

namespace PageObjectWrapper.AutomationUI
{
    public abstract class PageObjectValidationBase<T> : IDisposable
        where T : IAutomationPageObject
    {
        /// <summary>
        /// The base page to execute any WebElement operation
        /// </summary>
        protected PageObjectBase<T> PageBase { get; set; }

        public IList<By> ExceptionFields { get; set; }

        private List<PageObjectValidationData> ValidationData { get; set; }
        private List<PageObjectValidationError> ValidationErrors { get; set; }

        public bool HasErrors
        {
            get { return ValidationErrors != null && ValidationErrors.Any(); }
        }

        /// <summary>
        /// Virtual property to load the detail tables section from the page object.
        /// </summary>
        protected virtual IReadOnlyCollection<IWebElement> DetailTables
        {
            get
            {
                return PageBase.WebDriver.TryFindElements(By.XPath("//table[@class = 'detailList']"), TimeSpan.FromSeconds(5));
            }
        }

        /// <summary>
        /// Virtual property to load the headers for detail tables section at the page object.
        /// </summary>
        protected virtual IReadOnlyCollection<IWebElement> DetailTablesHeaders
        {
            get
            {
                return PageBase.WebDriver.TryFindElements(By.XPath("//div[@class = 'pbSubheader brandTertiaryBgr tertiaryPalette']//h3"), TimeSpan.FromSeconds(5));
            }
        }

        /// <summary>
        /// Virtual property. Gets the columns of the related list table.
        /// </summary>
        protected virtual string RelatedListBodyTableColumnsPart
        {
            get { return "descendant::tr[contains(@class, 'headerRow')]//th[not(contains(@class, 'actionColumn'))]"; }
        }

        /// <summary>
        /// Virtual property. Gets the rows of the related list table.
        /// </summary>
        protected virtual string RelatedListBodyTableRowsPart
        {
            get { return "descendant::tr[contains(@class, 'dataRow')]"; }
        }

        /// <summary>
        /// Virtual property. Gets the row data of the related list table.
        /// </summary>
        public string RelatedListBodyTableRowDataPart
        {
            get { return "child::*[not(contains(@class, 'actionColumn'))]"; }
        }

        /// <summary>
        /// Virtual property. Gets the related list table's titles.
        /// </summary>
        public By RelatedListHeaderTitles
        {
            get
            {
                return By.XPath("//*[contains(@class, 'bRelatedList')]/descendant::div[contains(@class, 'pbHeader')]/descendant::td[contains(@class, 'pbTitle')]//h3");
            }
        }

        /// <summary>
        /// Virtual property. Gets the columns of the related list's tables.
        /// </summary>
        public By RelatedListBodyTables
        {
            get
            {
                return By.XPath("//*[contains(@class, 'bRelatedList')]/descendant::div[contains(@class, 'pbBody')]/descendant::table");
            }
        }

        private DataSet _DSdetailTables;
        private DataSet _DSrelatedTables;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="pageBase">The base page object to store and execute WebElements operations.</param>
        public PageObjectValidationBase(PageObjectBase<T> pageBase)
        {
            PageBase = pageBase;
            ValidationData = new List<PageObjectValidationData>();
            ValidationErrors = new List<PageObjectValidationError>();
            ExceptionFields = new List<By>();
        }

        /// <summary>
        /// This method is supposed to validate all things related to layout on the page object.
        /// </summary>
        /// <returns>TRUE if all the layout validations are correct. Otherwise, FALSE.</returns>
        public virtual bool ValidateFields()
        {
            return false;
        }

        /// <summary>
        /// Fills all required fields for the specif page object before an attempt to save the case.
        /// </summary>
        public virtual void FillRequiredFields() { }

        /// <summary>
        /// It is recommended to encapsulate this method in a using(...) { } block.
        /// This is a safe statement code block to execute operations on the page object.
        /// After this block's execution, the ExceptionFields' property will be cleaned up.
        /// </summary>
        public virtual IDisposable UsingExceptionFieldList()
        {
            return this;
        }

        /// <summary>
        /// Virtual method to load all detail tables section from the page object.
        /// You can override this method if any special operation to load the tables is needed.
        /// </summary>
        /// <param name="rowTypeLoad">Determines how the rows will be loaded. Default is the WebElement's text.</param>
        /// <returns>A <see cref="DataSet"/> with all tables found on the page object.</returns>
        public virtual DataSet GetDetailTables(DataRowTypeLoad rowTypeLoad = DataRowTypeLoad.ElementText)
        {
            if (_DSdetailTables != null)
                return _DSdetailTables;

            _DSdetailTables = new DataSet();

            if (DetailTables != null && DetailTables.Any())
            {
                var index = 0;
                var tableNames = GetTableNames(DetailTables.Count);

                DetailTables.ToList().ForEach(table =>
                {
                    DataTable dt = CreateDataTable(table, tableNames[index], rowTypeLoad);
                    if (dt != null)
                    {
                        _DSdetailTables.Tables.Add(dt);
                    }
                    index++;
                });
            }

            return _DSdetailTables;
        }

        public virtual DataSet GetRelatedLists(DataRowTypeLoad rowTypeLoad = DataRowTypeLoad.ElementText)
        {
            if (_DSrelatedTables != null)
                return _DSrelatedTables;

            _DSrelatedTables = new DataSet();

            var relatedListTableElements = PageBase.WebDriver.FindElements(RelatedListBodyTables);
            var relatedListTitles = PageBase.WebDriver.FindElements(RelatedListHeaderTitles);

            if (relatedListTableElements != null && relatedListTableElements.Any())
            {
                var index = 0;

                relatedListTableElements.ToList().ForEach(table =>
                {
                    DataTable dt = CreateRelatedListDataTable(table, relatedListTitles[index].Text.Replace(" ", string.Empty), rowTypeLoad);
                    if (dt != null)
                    {
                        _DSrelatedTables.Tables.Add(dt);
                    }
                    index++;
                });
            }

            return _DSrelatedTables;
        }

        /// <summary>
        /// Gets the section header's names from page object to name the detail tables
        /// </summary>
        private string[] GetTableNames(int numberOfDetailTables)
        {
            List<string> result = new List<string>();

            var detailTablesHeaders = DetailTablesHeaders;
            var numberOfHeaders = detailTablesHeaders != null ? detailTablesHeaders.Count : 0;
            var mainTable = "MainTable";

            if (numberOfHeaders == numberOfDetailTables) // sometimes we have all the section's headers
            {
                detailTablesHeaders.ToList().ForEach(t => result.Add(t.Text.Trim().Replace(" ", string.Empty)));
            }
            else if (numberOfHeaders == (numberOfDetailTables - 1)) // in general, the main table in the page object does not have a section name
            {
                detailTablesHeaders.ToList().ForEach(t => result.Add(t.Text.Trim().Replace(" ", string.Empty)));
                result.Insert(0, mainTable);
            }
            else // if does not have headers at all, we provide generic names
            {
                for (int i = 0; i < numberOfDetailTables; i++)
                {
                    result.Add(string.Format("Table{0}", i + 1));
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Creates a <see cref="DataTable"/> object from a table element. The table's name will follow the index parameter.
        /// </summary>
        private DataTable CreateDataTable(IWebElement table, string tableName, DataRowTypeLoad rowTypeLoad)
        {
            var newDataTable = new DataTable(tableName);

            var rows = GetRows(table);
            var columns = GetColumns(rows);

            if (columns != null)
                columns.ToList().ForEach(col => newDataTable.Columns.Add(col, typeof(string)));

            if (columns == null || !columns.Any())
                return null;

            if (rows != null)
            {
                newDataTable.BeginLoadData();
                rows.ToList().ForEach(row =>
                {
                    var newRow = newDataTable.NewRow();
                    var itemArray = GetRowData(row, rowTypeLoad);

                    if (itemArray.Length > columns.Count())
                    {
                        newRow.ItemArray = itemArray.Take(columns.Count()).ToArray();
                    }
                    else
                    {
                        newRow.ItemArray = itemArray;
                    }

                    newDataTable.Rows.Add(newRow);
                });
                newDataTable.EndLoadData();
            }

            return newDataTable;
        }

        private DataTable CreateRelatedListDataTable(IWebElement table, string tableName, DataRowTypeLoad rowTypeLoad)
        {
            var newDT = new DataTable(tableName);

            var rows = GetRelatedListRows(table);
            var columns = GetRelatedListTableColumns(table);

            if (columns != null)
                columns.ToList().ForEach(col => newDT.Columns.Add(col, typeof(string)));

            if (columns == null || !columns.Any())
                return null;

            if (rows != null && rows.Any())
            {
                newDT.BeginLoadData();
                rows.ToList().ForEach(row =>
                {
                    var newRow = newDT.NewRow();
                    var itemArray = GetRelatedListRowData(row, rowTypeLoad);

                    if (itemArray.Length > columns.Count())
                    {
                        newRow.ItemArray = itemArray.Take(columns.Count()).ToArray();
                    }
                    else
                    {
                        newRow.ItemArray = itemArray;
                    }

                    newDT.Rows.Add(newRow);
                });
                newDT.EndLoadData();
            }

            return newDT;
        }

        /// <summary>
        /// Creates a <see cref="DataRow"/> for the table.
        /// </summary>
        private string[] GetRowData(IWebElement row, DataRowTypeLoad rowTypeLoad)
        {
            if (row == null)
                return new string[] { };

            var data = row.FindElements(By.XPath("td"));
            if (data != null)
            {
                return data.Select(x =>
                {
                    if (rowTypeLoad == DataRowTypeLoad.ElementText)
                        return x.IsSelectable() ? x.GetSelectedOptionText() : x.Text;

                    if (rowTypeLoad == DataRowTypeLoad.InnerHtml)
                        return x.GetAttribute("innerHTML");

                    //if (rowTypeLoad == DataRowTypeLoad.HtmlInnerText)
                    //    return x.GetAttribute("innerHTML").AsHtmlDocument().DocumentNode.InnerText;

                    return string.Empty;
                }).ToArray();
            }
            else
            {
                return new string[] { };
            }
        }

        private string[] GetRelatedListRowData(IWebElement row, DataRowTypeLoad rowTypeLoad)
        {
            if (row == null)
                return new string[] { };

            var data = row.FindElements(By.XPath(RelatedListBodyTableRowDataPart));
            if (data != null)
            {
                return data.Select(x =>
                {
                    if (rowTypeLoad == DataRowTypeLoad.ElementText)
                        return x.IsSelectable() ? x.GetSelectedOptionText() : x.Text;

                    if (rowTypeLoad == DataRowTypeLoad.InnerHtml)
                        return x.GetAttribute("innerHTML");

                    //if (rowTypeLoad == DataRowTypeLoad.HtmlInnerText)
                    //    return x.GetAttribute("innerHTML").AsHtmlDocument().DocumentNode.InnerText;

                    return string.Empty;
                }).ToArray();
            }
            else
            {
                return new string[] { };
            }
        }

        /// <summary>
        /// Gets all rows from the page object's element.
        /// </summary>
        private IReadOnlyCollection<IWebElement> GetRows(IWebElement element)
        {
            if (element == null)
                return null;

            return element.FindElements(By.XPath("tbody/tr"));
        }

        private IReadOnlyCollection<IWebElement> GetRelatedListRows(IWebElement table)
        {
            if (table == null)
                return null;

            return table.FindElements(By.XPath(RelatedListBodyTableRowsPart));
        }

        /// <summary>
        /// Gets all <thead> elements to use as reference for the table's columns names
        /// </summary>
        private IEnumerable<string> GetColumns(IReadOnlyCollection<IWebElement> rows)
        {
            if (rows != null && rows.Any())
            {
                var firstRow = rows.First();
                if (firstRow == null)
                    return null;

                var tds = firstRow.FindElements(By.XPath("td"));
                if (tds == null || tds.Count <= 0)
                    return null;

                var index = 0;
                return tds.Select(td =>
                {
                    index++;
                    return string.Format("col{0}", index);
                }).ToList();
            }

            return null;
        }

        private IEnumerable<string> GetRelatedListTableColumns(IWebElement table)
        {
            if (table == null)
                return null;

            return table.FindElements(By.XPath(RelatedListBodyTableColumnsPart)).Select(element => element.Text).ToList();
        }

        public PageObjectValidationBase<T> SetExceptionField(Func<T, By> selector)
        {
            T page = (T)Convert.ChangeType(PageBase, typeof(T));
            ExceptionFields.Add(selector(page));
            return this;
        }

        public PageObjectValidationBase<T> ExistsElementByFieldLabel(string fieldLabel, out bool result)
        {
            result = false;

            var props = PageBase.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).Where(p => Attribute.IsDefined(p, typeof(DescriptionAttribute))).ToList();
            var property = props.FirstOrDefault(p =>
            {
                var descAttrib = (DescriptionAttribute)Attribute.GetCustomAttribute(p, typeof(DescriptionAttribute));
                return (descAttrib != null && descAttrib.Description.Equals(fieldLabel));
            });

            result = property != null;
            return this;
        }

        public PageObjectValidationBase<T> SetExceptionField(string fieldLabel)
        {
            var props = PageBase.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).Where(p => Attribute.IsDefined(p, typeof(DescriptionAttribute))).ToList();
            var property = props.FirstOrDefault(p =>
            {
                var descAttrib = (DescriptionAttribute)Attribute.GetCustomAttribute(p, typeof(DescriptionAttribute));
                return (descAttrib != null && descAttrib.Description.Equals(fieldLabel));
            });

            if (property != null)
                ExceptionFields.Add(property.GetValue(PageBase) as By);

            return this;
        }

        public PageObjectValidationBase<T> ClearExceptionFields()
        {
            ExceptionFields.Clear();
            return this;
        }

        public PageObjectValidationBase<T> SetValidationData(string elementName, string expectedValue, Func<T, By> selector)
        {
            T page = (T)Convert.ChangeType(PageBase, typeof(T));
            SetValidationData(elementName, expectedValue, selector(page));
            return this;
        }

        public PageObjectValidationBase<T> SetValidationData(string elementName, string expectedValue, By selector)
        {
            ValidationData.Add(new PageObjectValidationData() { ElementName = elementName, ElementValue = expectedValue, Selector = selector });
            return this;
        }

        public void ValidateByValidationData()
        {
            foreach (var item in ValidationData)
            {
                if (item.Selector == null)
                {
                    ValidationErrors.Add(new PageObjectValidationError() { Code = 99, ErrorMessage = string.Format("The element {0} does not have a selector.", item.ElementName) });
                    continue;
                }

                var element = PageBase.WebDriver.FindElement(item.Selector);
                if (element.IsSelectable())
                {
                    var currentValue = element.GetSelectedOptionText();
                    if (!currentValue.Equals(item.ElementValue))
                        ValidationErrors.Add(new PageObjectValidationError() { Code = 1, ErrorMessage = string.Format("The element {0} is not valid. Expected value: {1} / Current value: {2}", item.ElementName, item.ElementValue, currentValue) });
                }
                else
                {
                    var currentValue = element.Text;
                    if (!currentValue.Equals(item.ElementValue))
                        ValidationErrors.Add(new PageObjectValidationError() { Code = 1, ErrorMessage = string.Format("The element {0} is not valid. Expected value: {1} / Current value: {2}", item.ElementName, item.ElementValue, currentValue) });
                }
            }

            ValidationData.Clear();
        }

        public string GetErrors()
        {
            if (!HasErrors)
                return null;

            var sb = new StringBuilder();

            foreach (var item in ValidationErrors)
                sb.AppendLine(string.Format("Error Code: {0}. Details: {1}", item.Code, item.ErrorMessage));

            return sb.ToString();
        }

        public void Dispose()
        {
            ClearExceptionFields();
        }
    }

    /// <summary>
    /// The loading types for the tables from page object validation
    /// </summary>
    public enum DataRowTypeLoad
    {
        /// <summary>
        /// Gets the <see cref="IWebElement"/> text string
        /// </summary>
        ElementText,

        /// <summary>
        /// Gets the <see cref="IWebElement"/> inner html attribute
        /// </summary>
        InnerHtml,

        /// <summary>
        /// Gets the <see cref="IWebElement"/> as a parsed html and returns the inner text
        /// </summary>
        HtmlInnerText,
    }

    public class PageObjectValidationData
    {
        public string ElementName { get; set; }
        public string ElementValue { get; set; }
        public By Selector { get; set; }
    }

    public class PageObjectValidationError
    {
        public int Code { get; set; }
        public string ErrorMessage { get; set; }
    }
}
