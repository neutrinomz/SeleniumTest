// See https://aka.ms/new-console-template for more information

using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Shouldly;
using OpenQA.Selenium.Support.UI;
using System;
using SeleniumExtras.WaitHelpers;

namespace Seleniumtest_zubkov;

public class SleniumTest

{
    public ChromeDriver driver;
    public WebDriverWait wait;

    public (string,string) CreateCommunity()
    {   
        // Переходим на страницу списка сообществ
        driver.Navigate().GoToUrl("https://staff-testing.testkontur.ru/communities");
        //Создаем сообщество
        var createButton =
            wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button.sc-juXuNZ.sc-ecQkzk.WTxfS.vPeNx")));
        createButton.Click();
        var namecommunity = driver.FindElement(By.CssSelector("textarea.react-ui-seuwan"));
        //Генерируем название сообщества 
        string uuid = Guid.NewGuid().ToString();
        namecommunity.SendKeys(uuid);
        var createCommunity = driver.FindElement(By.CssSelector("button.react-ui-m0adju"));
        createCommunity.Click();
        var deleteButton = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("button[data-tid='DeleteButton']")));
        var communiturl = driver.Url;
        //Возвращаем название сообщества и url
        return (uuid, communiturl); 
    }
 
    //Желательно добавлить OneTimeSetUp в котором через Базу Данных удалять все сообщества, чтобы тесты проходили на чистом окружении
    [SetUp]
    public void SetUp()
    {   
        //Задаем настройки браузера
        var options = new ChromeOptions();
        options.AddArguments("--no-sandbox", "--start-maximized", "--disable-extensions");
        driver = new ChromeDriver(options);
        // Устанавливаем неявное ожидание
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        // Устанавливаем явное ожидание
        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
        //Авторизация
        driver.Navigate().GoToUrl("https://staff-testing.testkontur.ru/");
        wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("Username")));
        var login = driver.FindElement(By.Id("Username"));
        login.SendKeys("zubkoff.m@yandex.ru");
        var password = driver.FindElement(By.Id("Password"));
        password.SendKeys("Userzubkov1!");
        var button = driver.FindElement(By.Name("button"));
        button.Click();
        //Проверяем, что кнопка "Войти" не отображается
        wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.Name("button")));
    }
    
    
    [Test]
    public void Autorize()
    {
        //Проверка, что мы авторизавались
        var currentUrl = driver.Url;
        Assert.That(currentUrl == "https://staff-testing.testkontur.ru/news",
        "current url = " + currentUrl + "а должен быть https://staff-testing.testkontur.ru/news");
        
    }

    [Test] 
    public void OpenListCommunities()
    {
        //Проверяем, что страница "Сообщетсва" работает
        var communityPage = driver.FindElement(By.CssSelector("a[data-tid='Community']"));
        communityPage.Click();
        var communitiesTitle = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("h1[data-tid='Title']")));
        //Достаем текст Title
        var communitiesTitleText = communitiesTitle.Text.Trim();
        Assert.That(communitiesTitleText, Is.EqualTo("Сообщества"), "Заголовок страницы не соответствует ожидаемому");
    }
    
    [Test] 
    public void FindUsers()
    {
        //Проверяем, что поиск пользователей работает
        // Ожидание появления строки поиска
        var searchBar = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("[data-tid='SearchBar']")));
        searchBar.Click();
        var input = driver.FindElement(By.CssSelector("label.react-ui-yydoep input.react-ui-1oilwm3"));
     
        // Ввод текста в строку поиска
        input.SendKeys("Макс");
        //Проверяем появление div с результатами поиска
        var searchResult = driver.FindElement(By.CssSelector("div.react-ui-1c042hz"));
        //Достаем заголовки результатов поиска
        var titles = driver.FindElements(By.CssSelector("div.sc-cOifOu.sc-gzcbmu.haQoIp.fKwyEY"));
        //Проверяем, что пользователь, который присутсвует в системе, находится
        bool isTitleFound = false;
        foreach (var title in titles)
        {
            if (title.Text == "Максим Зубков")
            {
                isTitleFound = true;
                break;
            }
        }
        //Если пользователь не найден, ошибка 
        Assert.That(isTitleFound, Is.True, "Заголовок 'Максим Зубков' не найден");
    }
    
    [Test]
    public void CheckСommunity()
    {
        //Проверяем, что сообщество отображается в списке сообществ на странице "Сообщества"
        //Получаем переменную uuid из метода "CreateCommunity"
        var communityName = CreateCommunity().Item1;
        // Переходим на страницу списка сообществ
        driver.Navigate().GoToUrl("https://staff-testing.testkontur.ru/communities");
        //Проверяем, что мы перешли на страницу "Сообщества" 
        var сheck = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("section.sc-fujyAs.sc-ojivU.cZJxtI.jjQalL")));
        var communityList = driver.FindElements(By.LinkText(communityName));
        //Если communityName не найден, ошибка
        Assert.That(communityList.Count > 0, Is.True, $"Элемент с UUID '{communityName}' не найден.");
    } 
    
    [Test]
    public void DeleteСommunity()
    { 
        //Проверяем, что сообщество удаляется
        //Получаем переменную uuid из метода "CreateCommunity"
        var communityName = CreateCommunity().Item1;
        //Удаление сообщества
        var deleteButton = driver.FindElement(By.CssSelector("button[data-tid='DeleteButton']"));
        deleteButton.Click();
        var confirmButton = driver.FindElement(By.CssSelector("div.react-ui-j884du"));
        confirmButton.Click();
        // Переходим на страницу списка сообществ
        driver.Navigate().GoToUrl("https://staff-testing.testkontur.ru/communities");
        //Проверяем, что страница загрузилась
        var check = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("section.sc-fujyAs.sc-ojivU.cZJxtI.jjQalL")));
        var communitylist = driver.FindElements(By.LinkText(communityName));
        //Проверяем, что сообщетсво не отображается в списке сообществ
        Assert.That(communitylist.Count == 0, Is.True, $"Сообщество с именем '{communityName}' не удалено.");
    }

    [TearDown]
    public void TearDown()
    {
        driver.Quit();
    }

}
