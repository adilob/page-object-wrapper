using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using System;
using System.Configuration;
using System.IO;

namespace PageObjectWrapper.AutomationUI
{
    [TestClass]
    public class MyWebUIMsTestBase
    {
        private static object _lockObj = new Object();

        private static IWebDriver _currentWebDriver = null;
        public static IWebDriver CurrentWebDriver
        {
            get
            {
                return _currentWebDriver;
            }
            set
            {
                lock (_lockObj)
                {
                    if (_currentWebDriver == null)
                    {
                        lock(_lockObj)
                        {
                            _currentWebDriver = value;
                        }
                    }
                }
            }
        }

        public IWebDriver TestWebDriver { get; set; }

        protected MyWebUIMsTestBase() { InitializeTestWebDriver(); }

        public MyWebUIMsTestBase(IWebDriver webDriver) { InitializeTestWebDriver(); }

        protected void InitializeTestWebDriver()
        {
            lock (_lockObj)
            {
                if (CurrentWebDriver != null)
                {
                    TestWebDriver = CurrentWebDriver;
                    return;
                }

                if (TestWebDriver == null)
                {
                    var configValue = ConfigurationManager.AppSettings["WebDriver"];

                    var path = Environment.CurrentDirectory;
                    path = Path.Combine(path, "WebDrivers");

                    switch (configValue)
                    {
                        case "Chrome":
                            path = Path.Combine(path, "Selenium.Chrome.WebDriver", "driver");
                            TestWebDriver = new ChromeDriver(path);
                            break;
                        case "InternetExplorer":
                            path = Path.Combine(path, "Selenium.WebDriver.IEDriver", "driver");
                            TestWebDriver = new InternetExplorerDriver(path);
                            break;
                        case "Firefox":
                            path = Path.Combine(path, "Selenium.Firefox.WebDriver", "driver");
                            TestWebDriver = new FirefoxDriver(path);
                            break;
                        default:
                            TestWebDriver = null;
                            break;
                    }
                }

                CurrentWebDriver = TestWebDriver;
            }
        }
    }
}
