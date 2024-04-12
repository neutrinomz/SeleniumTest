// See https://aka.ms/new-console-template for more information

using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Seleniumtest_zubkov;
    
public class Seleniumtest
{
    [Test]
    public void Authorisation()
    {
        var options = new ChromeOptions();
        options.AddArguments("--no-sandbox", "--start-maxinized", "--disable-extensions");
        var driver = new ChromeDriver(options);
        driver.Navigate().GoToUrl("https://staff-testing.testkontur.ru/");
        Thread.Sleep(3000);
        var login = driver.FindElement(By.Id("Username"));
        login.SendKeys("zubkoff.m@yandex.ru");
        var password = driver.FindElement(By.Id("Password"));
        password.SendKeys("41260Trat31193.");
        Thread.Sleep(3000);
        var button = driver.FindElement(By.Name("button"));
        button.Click();
        Thread.Sleep(3000);
        var currenturl = driver.Url;
        Assert.That(currenturl == "https://staff-testing.testkontur.ru/news");
        driver.Quit();
    }
}
