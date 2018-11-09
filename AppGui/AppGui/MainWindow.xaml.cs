﻿using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using mmisharp;
using Newtonsoft.Json;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace AppGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private MmiCommunication mmiC;
        private double ZoomValue = 100;
        private double ZoomIncrement = 10;
        private int scrollIncrement = 200;
        private IWebDriver driver;
        private IJavaScriptExecutor js;
        private ArrayList tabs = new ArrayList();
        private int tabCounter = 1;
        private String defaultUrl = "http://www.google.pt";

        public MainWindow()
        {

            InitializeComponent();
            InitializeChrome();

            mmiC = new MmiCommunication("localhost", 8000, "User1", "GUI");
            mmiC.Message += MmiC_Message;
            mmiC.Start();
        }

        private void InitializeChrome()
        {
            driver = new ChromeDriver(Environment.CurrentDirectory);
            driver.Manage().Window.Maximize();
            driver.Url = defaultUrl;
        }

        private void QuitChrome()
        {
            tabs.Clear();
            tabCounter = 1;
            driver.Quit();
        }

        private void changeInitialPage(String url)
        {
            defaultUrl = url;
            Console.WriteLine(defaultUrl);
        }
        private void CloseTab() {
            driver.Close();
        }
        private void NewTab(string tabName) 
        {
            js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("window.open("+"'"+ defaultUrl +"'" + "," + "'"+ tabName +"');");
            tabs.Add(tabName);
            driver.SwitchTo().Window(tabName);
        }

        private void OpenIncognitoTab()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArguments("--incognito");
            IWebDriver driver2 = new ChromeDriver(Environment.CurrentDirectory, options);
            driver2.Url=defaultUrl;

        }
        private String TabName()
        {
            while (tabs.Contains("tab" + tabCounter))
            {
                tabCounter++;
            }
            return "tab" + tabCounter;
        }

        private void ZoomIn()
        {
            ZoomValue += ZoomIncrement;
            Zoom(ZoomValue);
        }

        private void ZoomOut()
        {
            ZoomValue -= ZoomIncrement;
            Zoom(ZoomValue);
        }

        private void Zoom(double level)
        {
            js = (IJavaScriptExecutor)driver;
            Console.WriteLine(level.ToString().Replace(',', '.'));
            js.ExecuteScript(string.Format("document.body.style.zoom='{0}%'", level.ToString().Replace(',', '.')));
        }
        private void Search()
        {
            NewTab(TabName());
            js = (IJavaScriptExecutor)driver;
            IWebElement search = driver.FindElement(By.Name("q"));
            search.SendKeys("sinónimos de bolacha");
            Actions actions = new Actions(driver);
            IWebElement feelingLuckyButton = null;
            for (int i = 2; i < 12; i++)
            {

                try
                {
                    feelingLuckyButton = driver.FindElement(By.XPath("//*[@id='sbtc']/div[2]/div[2]/div[1]/div/ul/li[" + i + "]/div/span[2]/span/input"));
                }
                catch (Exception)
                {
                }
                if (feelingLuckyButton == null && i == 11)
                {
                    feelingLuckyButton = driver.FindElement(By.XPath("//*[@id='tsf']/div[2]/div[3]/center/input[2]"));
                }
                else if (feelingLuckyButton == null)
                {
                    continue;
                }
                else
                {
                    break;
                }
            }
            actions.MoveToElement(feelingLuckyButton).Click().Perform();
        }



        //Scroll down of the page
        private void ScrollDown()
        {
            js.ExecuteScript(String.Format("window.scrollBy(0,{0});", scrollIncrement));
        }

        private void ScrollUp()
        {
            js.ExecuteScript(String.Format("window.scrollBy(0,{0})", -scrollIncrement));
        }

        private void ScrollTop()
        {
            js.ExecuteScript("window.scrollTo(0, 0);");
        }

        private void ScrollBottom()
        {
            js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
        }


        private void MmiC_Message(object sender, MmiEventArgs e)
        {
            Console.WriteLine(e.Message);
            var doc = XDocument.Parse(e.Message);
            var com = doc.Descendants("command").FirstOrDefault().Value;
            dynamic json = JsonConvert.DeserializeObject(com);

            switch ((string)json.recognized[0].ToString())
            {
                case "NEW_TAB":
                    NewTab(TabName());
                    break;
                case "SEARCH":
                    Search();
                    break;
                case "ZOOM_IN":
                    ZoomIn();
                    break;
                case "ZOOM_OUT":
                    ZoomOut();
                    break;
                case "SCROLL_UP":
                    ScrollUp();
                    break;
                case "SCROLL_DOWN":
                    ScrollDown();
                    break;
                case "BOTTOM":
                    ScrollBottom();
                    break;
                case "TOP":
                    ScrollTop();
                    break;
                case "CLOSE_CHROME":
                    QuitChrome();
                    break;
            }

            /*
            App.Current.Dispatcher.Invoke(() =>
            {
                switch ((string)json.recognized[1].ToString())
                {
                    case "GREEN":
                        _s.Fill = Brushes.Green;
                        break;
                    case "BLUE":
                        _s.Fill = Brushes.Blue;
                        break;
                    case "RED":
                        _s.Fill = Brushes.Red;
                        break;
                    case "BLACK":
                        _s.Fill = Brushes.Black;
                        break;
                    case "YELLOW":
                        _s.Fill = Brushes.Yellow;
                        break;
                }
            });
            */




        }
    }
}
