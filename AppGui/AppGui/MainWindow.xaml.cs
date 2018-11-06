using System;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using mmisharp;
using Newtonsoft.Json;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium;
namespace AppGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MmiCommunication mmiC;
        public MainWindow()
        {
            InitializeComponent();


            mmiC = new MmiCommunication("localhost",8000, "User1", "GUI");
            mmiC.Message += MmiC_Message;
            mmiC.Start();

            IWebDriver driver = new FirefoxDriver();
            driver = new FirefoxDriver(Environment.CurrentDirectory);
            driver.Url = "http://www.demoqa.com";

        }

        private void MmiC_Message(object sender, MmiEventArgs e)
        {
            Console.WriteLine(e.Message);
            var doc = XDocument.Parse(e.Message);
            var com = doc.Descendants("command").FirstOrDefault().Value;
            dynamic json = JsonConvert.DeserializeObject(com);
            /*
            Shape _s = null;
            switch ((string)json.recognized[0].ToString())
            {
                case "SQUARE": _s = rectangle;
                    break;
                case "CIRCLE": _s = circle;
                    break;
                case "TRIANGLE": _s = triangle;
                    break;
            }

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
