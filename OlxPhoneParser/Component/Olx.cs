using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Homebrew;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;

namespace OlxPhoneParser.Component
{
    public class Olx
    {
        private RichTextBox _debugConsole;
        private string _category;
        private List<string> _allLinks = new List<string>();
        private Dictionary<string, string> allInfo = new Dictionary<string, string>();
        public Olx(string category, RichTextBox debugConsole)
        {
            _debugConsole = debugConsole;
            _category = category;
        }
        public void StartParsing()
        {
            _debugConsole.WriteLine("Начинаем парсинг указанной категории.");
            _category = _category.Contains("?") ? _category + "&search%5Bfilter_enum_state%5D%5B0%5D=new" : _category + "?search%5Bfilter_enum_state%5D%5B0%5D=new";
            int endPage = GetEndPage();
            _debugConsole.WriteLine($"Для парсинга доступно: {endPage.ToString()} страниц.");
            ParsAllAdLinks(endPage);
            _debugConsole.WriteLine($"Все категории спаршены.\nНайдено {_allLinks.Count} ссылок.\nНачинаем парсинг всех объявлений.");
            ParsAllInfo();
            Thread thread = new Thread(() =>
            {

            });
            thread.IsBackground = true;
            thread.Start();
        }
        private int GetEndPage()
        {
            ReqParametres req = new ReqParametres(_category);
            req.SetUserAgent("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.53 Safari/537.36");
            LinkParser linkParser = new LinkParser(req.Request);
            List<string> allLinks = linkParser.Data.ParsRegex("<a class=\"block br3 brc8 large tdnone lheight24\" href=\"(.*?)\"",1);
            string lastLink = allLinks[allLinks.Count - 1];
            int lastNumber = int.Parse(lastLink.ParsFromToEnd("page="));
            return lastNumber;
        }
        private void ParsAllAdLinks(int endPage)
        {
            for (int i = endPage-1; endPage>=i; i++)
            {
                ReqParametres req = new ReqParametres($"{_category}&page={i}");
                req.SetUserAgent("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.53 Safari/537.36");
                LinkParser linkParser = new LinkParser(req.Request);
                List<string> allRawLinks = linkParser.Data.Replace("\n", "").ParsRegex("<h3 class=\"lheight22 margintop5\">(.*?)class=", 1);
                List<string> allLinks = new List<string>();
                allRawLinks.ForEach(rowLink =>
                {
                    string clearLink = rowLink.ParsFromTo("href=\"","\"");
                    if (!clearLink.Equals(""))
                    {
                        allLinks.Add(clearLink);
                    }
                });
                _allLinks.AddRange(allLinks);
                _debugConsole.WriteLine($"Страница {i} из {endPage} спаршена. Найдено {allLinks.Count} объявлений.");
            }
        }
        int currentPosition = 0;
        private void ParsAllInfo()
        {
            for (int i = 0; 10>i; i++)
            {
                Thread thread = new Thread(() =>
                {
                    string proxy = GetValidProxy();
                    CreateSelenium(proxy);
                });
                thread.IsBackground = true;
                thread.Start();
            }
        }
        private void CreateSelenium(string externalProxy)
        {
            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            ChromeOptions options = new ChromeOptions();
            options.AddUserProfilePreference("profile.default_content_setting_values.images", 2);
            Proxy proxy = new Proxy();
            proxy.Kind = ProxyKind.Manual;
            proxy.IsAutoDetect = false;
            proxy.HttpProxy =
            proxy.SslProxy = externalProxy;
            options.AddArgument("ignore-certificate-errors");
            options.PageLoadStrategy = PageLoadStrategy.Eager;
            options.AddArguments(new List<string>() { "no-sandbox", "disable-gpu" });
            options.Proxy = proxy;
            IWebDriver webDriver = new ChromeDriver(driverService,options);
            webDriver.Manage().Window.Minimize();
            int workLink = currentPosition++;
            while (workLink < _allLinks.Count)
            {
                try
                {
                    webDriver.Navigate().GoToUrl(_allLinks[workLink]);
                    if (webDriver.FindElements(By.CssSelector(".cookie-close.abs.cookiesBarClose")).Count == 0)
                    {
                        throw new Exception();
                    }
                    //var cookieAlert = webDriver.FindElement(By.CssSelector(".cookie-close.abs.cookiesBarClose"));
                    //cookieAlert.Click();

                    IWebElement phone = null;
                    IWebElement name = null;
                    
                    if (webDriver.FindElements(By.ClassName("form")).Count == 0 || webDriver.FindElement(By.ClassName("form")).Text.Equals(""))
                    {
                        workLink = currentPosition++;
                        continue;
                    }
                    var webElement = webDriver.FindElement(By.ClassName("form"));


                    Actions actions = new Actions(webDriver);
                    actions.MoveToElement(webElement);
                    actions.Perform();
                    ((IJavaScriptExecutor)webDriver).ExecuteScript("window.scrollBy(0, 350)");

                    webElement.Click();
                    name = webDriver.FindElement(By.ClassName("offer-user__details"));
                    phone = webDriver.FindElement(By.CssSelector(".fnormal.xx-large"));
                    int chance = 0;
                    while (phone.Text.Contains("xxx")&&chance++<10)
                    {
                        webElement.Click();
                        Thread.Sleep(100);
                        phone = webDriver.FindElement(By.CssSelector(".fnormal.xx-large"));
                    }

                    if (phone.Text.Contains("xxx"))
                    {
                        throw new Exception();
                    }
                    string realName = "";
                    List<string> allTextName = name?.Text.ParsRegex("([а-яА-Яa-zA-Z0-9]+)", 1);
                    if (allTextName.Count > 0)
                    {
                        realName = allTextName[0];
                    }
                    List<string> allPhones = new List<string>(phone.Text.Split('\n'));
                    allPhones.ForEach(ph =>
                    {
                        ph.Replace("\n", "");
                        realName.Replace("\n", "");
                        if (!allInfo.ContainsKey(ph))
                        {
                            allInfo.Add(ph, realName);
                            _debugConsole.WriteLine(ph + " : " + realName);
                        }
                    });
                    workLink = currentPosition++;
                    webDriver.Manage().Cookies.DeleteAllCookies();
                }
                catch (Exception ex)
                {
                    webDriver.Quit();
                    proxy = new Proxy();
                    proxy.Kind = ProxyKind.Manual;
                    proxy.IsAutoDetect = false;
                    proxy.HttpProxy =
                    proxy.SslProxy = externalProxy;
                    options.AddArgument("ignore-certificate-errors");
                    options.AddArguments(new List<string>() { "no-sandbox", "disable-gpu" });
                    options.Proxy = proxy;
                    webDriver = new ChromeDriver(driverService,options);
                    webDriver.Manage().Window.Minimize();
                }
            }
            
            webDriver?.Quit();
        }
        private string GetValidProxy()
        {
            string proxy = "";
            bool isValid = false;
            while (!isValid)
            {
                try
                {
                    proxy = Proxies.GetProxy();
                    ReqParametres reqParametres = new ReqParametres("https://www.olx.ua/");
                    reqParametres.Request.Timeout = 5000;
                    reqParametres.Request.Proxy = new WebProxy(proxy,false);
                    reqParametres.SetUserAgent("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.53 Safari/537.36");
                    LinkParser linkParser = new LinkParser(reqParametres.Request);
                    if (linkParser.Data.Contains("Детский мир"))
                    {
                        isValid = true;
                    }
                    else
                    {
                        isValid = false;
                    }
                }
                catch (Exception ex)
                {
                    isValid = false;
                }
            }
            return proxy;

        }
    }
}
