namespace Tests
{
    using NUnit.Framework;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Appium;
    using OpenQA.Selenium.Appium.Windows;
    using OpenQA.Selenium.Interactions;
    using Renci.SshNet;
    using System;
    using System.IO;
    using System.Threading;
    using System.Configuration;


    public class WinAppDriverDemoTest
    {


        public string env = Environment.MachineName;
        public string currentEnv = "local"; //local, jenkins, teamcity
        public string currentDir = Directory.GetCurrentDirectory();
        public string publicIp = "";


        protected static WindowsDriver<WindowsElement> driver;
        protected static WindowsElement editBox;


        [SetUp]
        public void Setup()
        {
            Console.WriteLine("###################Setup start");
            Console.WriteLine("current running in which environment = " + env);
            publicIp = ConfigurationManager.AppSettings["PublicIp"];
            if (env.Contains("mxie"))
            {
                currentEnv = "local";
                if (currentDir.Contains("jenkins"))
                {
                    currentEnv = "jenkins";
                }
                if (currentDir.Contains("builds"))
                {
                    currentEnv = "teamcity";

                }

            }

            Console.WriteLine("currentEnv = " + currentEnv);



        }

        [Test]
        public void WinAppDriverInstallCygwinTest()
        {

            //OpenNotepad(publicIp);
            InstallCygwin(publicIp);

        }


        //If you need to run WinAppDriver in other machine other than your local computer
        public void StartWinAppDriverServer(string publicIp)
        {
            using (var client = new SshClient(publicIp, "", ""))
            {
                client.Connect();
                var result = client.CreateCommand(@"start /B C:\Program Files\Windows Application Driver\WinAppDriver.exe --verbose > c:\users\support\winium.log").Execute();
                Console.Out.WriteLine("result=" + result);
                client.Disconnect();

            }
        }


        public void OpenNotepad(string publicIp)
        {
            string WindowsApplicationDriverUrl = "http://" + publicIp + ":4723";
            const string NotepadAppId = @"C:\Windows\System32\notepad.exe";

            if (driver == null)
            {
                // Create a new session to launch Notepad application
                //DesiredCapabilities appCapabilities = new DesiredCapabilities();  this was the old code which in the demo of the offical WinAppDriver 
                //appCapabilities.SetCapability("app", NotepadAppId);
                //appCapabilities.SetCapability("deviceName", "WindowsPC");

                AppiumOptions options = new AppiumOptions();
                options.AddAdditionalCapability("deviceName", "WindowsPC");
                options.AddAdditionalCapability("platformName", "Windows");
                options.AddAdditionalCapability("app", NotepadAppId);

                driver = new WindowsDriver<WindowsElement>(new Uri(WindowsApplicationDriverUrl), options);
                //session = new WindowsDriver<WindowsElement>(new Uri(WindowsApplicationDriverUrl), appCapabilities);  old code which is in the demo of the offical WinAppDriver

                Assert.IsNotNull(driver);
                Assert.IsNotNull(driver.SessionId);

                // Verify that Notepad is started with untitled new file
                Assert.AreEqual("Untitled - Notepad", driver.Title);

                // Set implicit timeout to 1.5 seconds to make element search to retry every 500 ms for at most three times
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1.5);

                // Keep track of the edit box to be used throughout the session
                editBox = driver.FindElementByClassName("Edit");
                Assert.IsNotNull(editBox);

                editBox.SendKeys("I am Michelle, I am going to installing cygwin");

                if (driver != null)
                {
                    driver.Close();

                    try
                    {
                        // Dismiss Save dialog if it is blocking the exit
                        driver.FindElementByName("Don't Save").Click();
                    }
                    catch { }

                    driver.Quit();
                    driver = null;
                }
            }


        }

        public void InstallCygwin(string publicIp)
        {
            string WindowsApplicationDriverUrl = "http://" + publicIp + ":4723";
            const string AppName = @"c:\cygwin-setup-x86_64.exe";
            const string InstallPath = @"C:\cygwin64";


            AppiumOptions options = new AppiumOptions();
            options.AddAdditionalCapability("deviceName", "WindowsPC");
            options.AddAdditionalCapability("platformName", "Windows");
            options.AddAdditionalCapability("app", AppName);

            driver = new WindowsDriver<WindowsElement>(new Uri(WindowsApplicationDriverUrl), options);

            Assert.IsNotNull(driver);
            Assert.IsNotNull(driver.SessionId);

            var windowSetup = driver.FindElementByName("Cygwin Setup");

            Actions action = new Actions(driver);

            Assert.IsNotNull(driver.FindElementsByName("Cygwin Setup - Choose Installation Type"));
            ScreenShot(driver, currentEnv);
            action.SendKeys(Keys.Alt + "N").Build().Perform();

            Assert.IsNotNull(driver.FindElementsByName("Cygwin Setup - Choose Installation Directory"));
            ScreenShot(driver, currentEnv);
            action.SendKeys(InstallPath).Build().Perform();
            action.SendKeys(Keys.Tab).Build().Perform();
            action.SendKeys(Keys.Tab).Build().Perform();
            action.SendKeys(Keys.Alt + "N").Build().Perform();

            Assert.IsNotNull(driver.FindElementsByName("Cygwin Setup - Select Local Package Directory"));
            ScreenShot(driver, currentEnv);
            Thread.Sleep(10000);
            action.SendKeys(InstallPath).Build().Perform();
            action.SendKeys(Keys.Tab).Build().Perform();
            action.SendKeys(Keys.Alt + "N").Build().Perform();

            Assert.IsNotNull(driver.FindElementsByName("Cygwin Setup - Select Connection Type"));
            ScreenShot(driver, currentEnv);
            action.SendKeys(Keys.Tab).Build().Perform();
            action.SendKeys(Keys.Alt + "N").Build().Perform();

            Assert.IsNotNull(driver.FindElementsByName("Cygwin Setup - Choose Download Site(s)"));
            ScreenShot(driver, currentEnv);
            action.SendKeys(Keys.Alt + "N").Build().Perform();

            Assert.IsNotNull(driver.FindElementsByName("Cygwin Setup - Select Packages"));
            ScreenShot(driver, currentEnv);
            action.SendKeys(Keys.Alt + "N").Build().Perform();

            Assert.IsNotNull(driver.FindElementsByName("Cygwin Setup - Review and confirm changes"));
            ScreenShot(driver, currentEnv);
            action.SendKeys(Keys.Alt + "N").Build().Perform();

            Assert.IsNotNull(driver.FindElementsByName("Cygwin Setup - Installation Status and Create Icons"));
            ScreenShot(driver, currentEnv);
            action.SendKeys(Keys.Tab).Build().Perform();
            action.SendKeys(Keys.Tab).Build().Perform();
            action.SendKeys(Keys.Tab).Build().Perform();
            driver.FindElementByName("Finish").Click();
            action.Release();

            if (driver != null)
            {
                driver.Close();
                driver.Quit();
                driver = null;
            }



        }


        public void ScreenShot(IWebDriver driver, string currentEnv, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {

            try
            {
                string ImageName = "Win10 pro";
                //Take the screenshot
                Screenshot image = ((ITakesScreenshot)driver).GetScreenshot();
                //Save the screenshot
                string currentDir = Directory.GetCurrentDirectory();
                string screenshotDir = "Screenshot";
                Console.WriteLine("current directory is " + currentDir);


                if (currentEnv.Equals("teamcity"))
                {
                    string solutionDir = currentDir.Substring(0, currentDir.IndexOf("UIAutomationTest"));
                    Console.WriteLine("screenshot directory is " + solutionDir);
                    screenshotDir = solutionDir + "artifacts\\screenshot\\";
                    Console.WriteLine("screenshot directory is " + screenshotDir);
                }
                
                // If directory does not exist, create it. 
                if (!Directory.Exists(screenshotDir))
                {
                    Directory.CreateDirectory(screenshotDir);
                }

                string screenshotSubDir = screenshotDir + ImageName + "-" + memberName;//new StackTrace().GetFrame(1).GetMethod().Name;
                if (!Directory.Exists(screenshotSubDir))
                {
                    Directory.CreateDirectory(screenshotSubDir);
                }

                image.SaveAsFile(screenshotSubDir + "\\" + DateTime.Now.ToString("dd-hh-mm-ss") + ".png", OpenQA.Selenium.ScreenshotImageFormat.Png);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Assert.Fail("Failed with Exception: " + e);
            }

        }
    }
}
