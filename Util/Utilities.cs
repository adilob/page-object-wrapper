using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace PageObjectWrapper.AutomationUI.Util
{
    static public class Utilities
    {
        public static void TakeSnapshot(TestContext TestContext, IWebDriver TestWebDriver, string testname)
        {
            Regex regex = new Regex("[^a-zA-Z0-9-_]");
            string normalizedName = regex.Replace(testname, "");
            string filename = "C:\\Temp\\" + normalizedName + "_" + System.DateTime.Now.ToString("dd_MMMM_hh_mm_ss_tt") + ".jpg";
            SaveFullSnapshot(TestWebDriver, filename);
            TestContext.AddResultFile(filename);
        }

        public static void TakeCurrentScreen(IWebDriver TestWebDriver)
        {
            string scenarioTitle = ScenarioContext.Current.ScenarioInfo.Title;
            Regex regex = new Regex("[^a-zA-Z0-9-_]");
            string normalizedName = regex.Replace(scenarioTitle, "");

            string filename = "C:\\Temp\\" + normalizedName + "_" + System.DateTime.Now.ToString("dd_MMMM_hh_mm_ss_tt") + ".jpg";
            ((OpenQA.Selenium.ITakesScreenshot)TestWebDriver).GetScreenshot().SaveAsFile(filename, OpenQA.Selenium.ScreenshotImageFormat.Jpeg);
            ScenarioContext.Current.ScenarioContainer.Resolve<Microsoft.VisualStudio.TestTools.UnitTesting.TestContext>().AddResultFile(filename);
        }

        public static void TakeSnapshot(IWebDriver TestWebDriver)
        {
            string scenarioTitle = ScenarioContext.Current.ScenarioInfo.Title;
            Regex regex = new Regex("[^a-zA-Z0-9-_]");
            string normalizedName = regex.Replace(scenarioTitle, "");

            string filename = "C:\\Temp\\" + normalizedName + "_" + System.DateTime.Now.ToString("dd_MMMM_hh_mm_ss_tt") + ".jpg";
            SaveFullSnapshot(TestWebDriver, filename);
            ScenarioContext.Current.ScenarioContainer.Resolve<Microsoft.VisualStudio.TestTools.UnitTesting.TestContext>().AddResultFile(filename);
        }

        private static void SaveFullSnapshot(IWebDriver TestWebDriver, string filename)
        {
            int totalWidth = (int)(long)((IJavaScriptExecutor)TestWebDriver).ExecuteScript("return document.body.scrollWidth");
            int totalHeight = (int)(long)((IJavaScriptExecutor)TestWebDriver).ExecuteScript("return document.body.parentNode.scrollHeight");

            int viewportWidth = (int)(long)((IJavaScriptExecutor)TestWebDriver).ExecuteScript("return document.documentElement.clientWidth");
            int viewportHeight = (int)(long)((IJavaScriptExecutor)TestWebDriver).ExecuteScript("return document.documentElement.clientHeight");

            if ((totalWidth <= viewportWidth) && totalHeight <= viewportHeight)
            {
                ((OpenQA.Selenium.ITakesScreenshot)TestWebDriver).GetScreenshot().SaveAsFile(filename, OpenQA.Selenium.ScreenshotImageFormat.Jpeg);
            }
            else
            {
                List<Rectangle> viewportsList = new List<Rectangle>();
                for (int w = 0; w < totalWidth; w += viewportWidth)
                {
                    int newWidth = viewportWidth;
                    if ((w + viewportWidth) > totalWidth)
                    {
                        newWidth = totalWidth - w;
                    }

                    for (int h = 0; h < totalHeight; h += viewportHeight)
                    {
                        int newHeight = viewportHeight;

                        if ((h + viewportHeight) > totalHeight)
                        {
                            newHeight = totalHeight - h;
                        }

                        Rectangle currentRect = new Rectangle(w, h, newWidth, newHeight);
                        viewportsList.Add(currentRect);

                    }
                }

                Bitmap fullImage = new Bitmap(totalWidth, totalHeight);
                Rectangle previousRect = Rectangle.Empty;

                foreach (Rectangle rect in viewportsList)
                {
                    if (previousRect != Rectangle.Empty)
                    {
                        int wDiff = rect.Right - previousRect.Right;
                        int hDiff = rect.Bottom - previousRect.Bottom;
                        ((IJavaScriptExecutor)TestWebDriver).ExecuteScript(String.Format("window.scrollBy({0}, {1})", wDiff, hDiff));
                    }

                    Screenshot currentScreenshot = ((OpenQA.Selenium.ITakesScreenshot)TestWebDriver).GetScreenshot();
                    Image currentImage = Image.FromStream(new MemoryStream(currentScreenshot.AsByteArray));

                    Rectangle sourceRectangle = new Rectangle(viewportWidth - rect.Width, viewportHeight - rect.Height, rect.Width, rect.Height);
                    Graphics.FromImage(fullImage).DrawImage(currentImage, rect, sourceRectangle, GraphicsUnit.Pixel);

                    previousRect = rect;
                }

                fullImage.Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }
    }
}
